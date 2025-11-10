using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Interfaces;

public interface IMarkerRepository
{
    //Task<Marker> GetByIdAsync(Guid id); 
    Task<IEnumerable<Marker>> GetAllAsync(); // Получить все маркеры. IEnumerable: коллекция, чтобы не загружать в память сразу все.
    Task AddAsync(Marker marker); // Добавить маркер. Не сохраняет сразу — для batch (несколько операций перед сохранением).
    //Task UpdateAsync(Marker marker); 
    Task SaveChangesAsync(); // Сохранить все изменения в БД. Отдельно: чтобы группировать операции (эффективнее, чем сохранять каждый раз).
}