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
    public class ExportService : IExportService
    {
        private readonly IMongoCollection<WarehouseExport> _exportsCollection;
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public ExportService(IMongoDatabase database)
        {
            _exportsCollection = database.GetCollection<WarehouseExport>("WarehouseExport");
            _booksCollection = database.GetCollection<Book>("Book");
            _usersCollection = database.GetCollection<User>("User");
        }

        public async Task<WarehouseExportDto> CreateExportAsync(WarehouseExportDto exportDto)
        {
            if (exportDto.WarehouseExportBooks == null || !exportDto.WarehouseExportBooks.Any())
                throw new ArgumentException("Danh sách sách xuất không được để trống");

            var user = await _usersCollection.Find(u => u.UserId == exportDto.UserId).FirstOrDefaultAsync();
            if (user == null)
                throw new ArgumentException($"Người dùng với UserId {exportDto.UserId} không tồn tại");

            var export = new WarehouseExport
            {
                ExportDate = exportDto.ExportDate,
                UserId = exportDto.UserId,
                WarehouseExportBooks = exportDto.WarehouseExportBooks.Select(eb => new WarehouseExportBook
                {
                    WarehouseExportId = exportDto.ExportId,
                    BookId = eb.BookId,
                    ExportQuantity = eb.ExportQuantity
                }).ToList()
            };

            foreach (var exportBook in export.WarehouseExportBooks)
            {
                if (string.IsNullOrEmpty(exportBook.BookId))
                    throw new ArgumentException("BookId không được để trống");

                var bookFilter = Builders<Book>.Filter.Eq(b => b.BookId, exportBook.BookId);
                var book = await _booksCollection.Find(bookFilter).FirstOrDefaultAsync();
                if (book == null)
                    throw new ArgumentException($"Sách với ID {exportBook.BookId} không tồn tại");

                if (book.Quantity < exportBook.ExportQuantity)
                    throw new ArgumentException($"Số lượng tồn kho của sách không đủ (còn {book.Quantity})");

                var update = Builders<Book>.Update.Inc(b => b.Quantity, -exportBook.ExportQuantity);
                await _booksCollection.UpdateOneAsync(bookFilter, update);
            }

            await _exportsCollection.InsertOneAsync(export);

            exportDto.ExportId = export.ExportId;
            return exportDto;
        }

        public async Task<List<WarehouseExportDto>> GetAllExportsAsync()
        {
            var exports = await _exportsCollection.Find(_ => true).ToListAsync();
            return exports.Select(e => new WarehouseExportDto
            {
                ExportId = e.ExportId,
                ExportDate = e.ExportDate,
                UserId = e.UserId,
                WarehouseExportBooks = e.WarehouseExportBooks.Select(eb => new WarehouseExportBookDto
                {
                    BookId = eb.BookId,
                    ExportQuantity = eb.ExportQuantity
                }).ToList()
            }).ToList();
        }
    }
}