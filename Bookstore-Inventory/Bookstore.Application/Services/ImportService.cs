using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Application.Services
{
    public class ImportService : IImportService
    {
        private readonly IMongoCollection<WarehouseImport> _importsCollection;
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public ImportService(IMongoDatabase database)
        {
            _importsCollection = database.GetCollection<WarehouseImport>("WarehouseImport");
            _booksCollection = database.GetCollection<Book>("Book");
            _usersCollection = database.GetCollection<User>("User");
        }

        public async Task<WarehouseImportDto> CreateImportAsync(WarehouseImportDto importDto)
        {
            if (importDto.WarehouseImportBooks == null || !importDto.WarehouseImportBooks.Any())
                throw new ArgumentException("Danh sách sách nhập không được để trống");

            var user = await _usersCollection.Find(u => u.UserId == importDto.UserId).FirstOrDefaultAsync();
            if (user == null)
                throw new ArgumentException($"Người dùng với UserId {importDto.UserId} không tồn tại");

            var import = new WarehouseImport
            {
                ImportDate = importDto.ImportDate,
                UserId = importDto.UserId,
                WarehouseImportBooks = importDto.WarehouseImportBooks.Select(ib => new WarehouseImportBook
                {
                    WarehouseImportId = importDto.ImportId,
                    BookId = ib.BookId,
                    ImportQuantity = ib.ImportQuantity
                }).ToList()
            };

            foreach (var importBook in import.WarehouseImportBooks)
            {
                if (string.IsNullOrEmpty(importBook.BookId))
                    throw new ArgumentException("BookId không được để trống");

                var bookFilter = Builders<Book>.Filter.Eq(b => b.BookId, importBook.BookId);
                var book = await _booksCollection.Find(bookFilter).FirstOrDefaultAsync();
                if (book == null)
                    throw new ArgumentException($"Sách với ID {importBook.BookId} không tồn tại");

                var update = Builders<Book>.Update.Inc(b => b.Quantity, importBook.ImportQuantity);
                await _booksCollection.UpdateOneAsync(bookFilter, update);
            }

            await _importsCollection.InsertOneAsync(import);

            importDto.ImportId = import.ImportId;
            return importDto;
        }

        public async Task<List<WarehouseImportDto>> GetAllImportsAsync()
        {
            var imports = await _importsCollection.Find(_ => true).ToListAsync();
            return imports.Select(i => new WarehouseImportDto
            {
                ImportId = i.ImportId,
                ImportDate = i.ImportDate,
                UserId = i.UserId,
                WarehouseImportBooks = i.WarehouseImportBooks.Select(ib => new WarehouseImportBookDto
                {
                    BookId = ib.BookId,
                    ImportQuantity = ib.ImportQuantity
                }).ToList()
            }).ToList();
        }
    }
}