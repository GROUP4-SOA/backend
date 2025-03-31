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

        public ExportService(IMongoDatabase database)
        {
            _exportsCollection = database.GetCollection<WarehouseExport>("WarehouseExports");
            _booksCollection = database.GetCollection<Book>("Books");
        }

        public async Task<WarehouseExportDto> CreateExportAsync(WarehouseExportDto exportDto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (exportDto.WarehouseExportBooks == null || !exportDto.WarehouseExportBooks.Any())
                throw new ArgumentException("Danh sách sách xuất không được để trống");

            // Tạo phiếu xuất kho
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

            // Kiểm tra và cập nhật số lượng tồn kho
            foreach (var exportBook in export.WarehouseExportBooks)
            {
                var bookFilter = Builders<Book>.Filter.Eq(b => b.BookId, exportBook.BookId);
                var book = await _booksCollection.Find(bookFilter).FirstOrDefaultAsync();
                if (book == null)
                    throw new ArgumentException($"Sách với ID {exportBook.BookId} không tồn tại");

                if (book.Quantity < exportBook.ExportQuantity)
                    throw new ArgumentException($"Số lượng tồn kho của sách không đủ (còn {book.Quantity})");

                var update = Builders<Book>.Update.Inc(b => b.Quantity, -exportBook.ExportQuantity);
                await _booksCollection.UpdateOneAsync(bookFilter, update);
            }

            // Lưu phiếu xuất kho vào MongoDB
            await _exportsCollection.InsertOneAsync(export);

            // Cập nhật ExportId trong DTO
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