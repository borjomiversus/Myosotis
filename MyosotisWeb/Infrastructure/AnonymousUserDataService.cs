using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyosotisWeb.Infrastructure
{
    public class AnonymousUserDataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _storageDirectory;
        private Guid? _cachedUserId;

        // блок на рівні користувача, щоб уникнути конфліктів при паралельних запитах
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Formatting = Formatting.Indented
        };

        public AnonymousUserDataService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var homePath = Environment.GetEnvironmentVariable("HOME");
            var basePath = !string.IsNullOrEmpty(homePath) ? Path.Combine(homePath, "data") : AppContext.BaseDirectory;

            _storageDirectory = Path.Combine(basePath, "App_Data", "AnonymousUsers");
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        public Guid GetCurrentUserId()
        {
            if (_cachedUserId.HasValue)
                return _cachedUserId.Value;

            var context = _httpContextAccessor.HttpContext;

            if (context != null &&
                context.Items.TryGetValue("AnonUserId", out var idObj) &&
                idObj is Guid userId &&
                userId != Guid.Empty)
            {
                _cachedUserId = userId;
                return userId;
            }

            throw new InvalidOperationException(
                "Anonymous user ID has not been initialized for this Blazor circuit.");
        }
        private string GetFilePath(Guid userId) => Path.Combine(_storageDirectory, $"{userId:N}.json");

        public async Task<User> GetUserDataAsync()
        {
            var userId = GetCurrentUserId();
            var filePath = GetFilePath(userId);
            var fileLock = _locks.GetOrAdd(
                userId,
                _ => new SemaphoreSlim(1, 1)
            );

            await fileLock.WaitAsync();

            try
            {
                if (!File.Exists(filePath))
                    return CreateNewUser();

                var json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidDataException(
                        $"User data file is empty: {userId}"
                    );
                }

                var storageDto =
                    JsonConvert.DeserializeObject<UserStorageDto>(
                        json,
                        _jsonSettings
                    );

                if (storageDto == null)
                {
                    throw new InvalidDataException(
                        $"User data could not be deserialized: {userId}"
                    );
                }

                return UserMapper.ToDomain(storageDto);
            }
            finally
            {
                fileLock.Release();
            }
        }

        public async Task SaveUserDataAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userId = GetCurrentUserId();
            var filePath = GetFilePath(userId);
            var tempPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            var fileLock = _locks.GetOrAdd(
                userId,
                _ => new SemaphoreSlim(1, 1)
            );

            await fileLock.WaitAsync();

            try
            {
                DateTime createdAt = DateTime.UtcNow;

                if (File.Exists(filePath))
                {
                    var existingJson = await File.ReadAllTextAsync(filePath);

                    var existingDto =
                        JsonConvert.DeserializeObject<UserStorageDto>(
                            existingJson,
                            _jsonSettings
                        );

                    if (existingDto == null)
                    {
                        throw new InvalidDataException(
                            $"Cannot overwrite invalid user data: {userId}"
                        );
                    }

                    createdAt = existingDto.CreatedAt;
                }

                var storageDto = UserMapper.ToDto(user, createdAt);

                var json = JsonConvert.SerializeObject(
                    storageDto,
                    _jsonSettings
                );

                await File.WriteAllTextAsync(tempPath, json);

                File.Move(tempPath, filePath, overwrite: true);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                throw;
            }
            finally
            {
                fileLock.Release();
            }
        }

        public async Task DeleteUserDataAsync()
        {
            var userId = GetCurrentUserId();
            var filePath = GetFilePath(userId);
            var fileLock = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

            await fileLock.WaitAsync();
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            finally
            {
                fileLock.Release();
            }
        }

        private User CreateNewUser() => new User("Guest");
    }
}