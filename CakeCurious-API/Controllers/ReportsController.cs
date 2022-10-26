using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Reports;
using Repository.Interfaces;
using Repository.Models.Product;
using Repository.Models.Reports;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IViolationReportRepository _ReportRepository;

        public ReportsController(IViolationReportRepository ReportRepository)
        {
            _ReportRepository = ReportRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<StaffDashboardReportPage>>> GetReports(string? search, string? sort, string? type, string? status, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new StaffDashboardReportPage();
            result.Reports = await _ReportRepository.GetViolationReports(search, sort, type, status, page, size);
            result.TotalPage = (int)Math.Ceiling((decimal)_ReportRepository.CountDashboardViolationReports(search, sort, type, status) / size);
            return Ok(result);
        }

        [HttpGet("item-detail/{guid}")]
        [Authorize]
        public async Task<ActionResult<ItemReportContent>> GetItemDetail(Guid? guid)
        {
            var result = await _ReportRepository.GetReportedItemDetail(guid);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetProductsById(Guid guid)
        {
            var result = await _ReportRepository.GetById(guid);
            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostReport(ViolationReport inputReport)
        {
            Guid id = Guid.NewGuid();
            ViolationReport report = new ViolationReport()
            {
                Id = id,
                Content = inputReport.Content,
                ItemType = inputReport.ItemType,
                ReporterId = inputReport.ReporterId,
                StaffId = inputReport.StaffId,
                ItemId = inputReport.ItemId,
                ReportCategoryId = inputReport.ReportCategoryId,
                Title = inputReport.Title,
                Status = inputReport.Status,
                SubmittedDate = inputReport.SubmittedDate,
            };
            try
            {
                _ReportRepository.Add(report);
            }
            catch (DbUpdateException)
            {
                if (_ReportRepository.GetById(inputReport.Id!.Value) != null)
                    return Conflict();
            }
            return Ok(report);
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutReport(Guid guid, ViolationReport inputReport)
        {
            try
            {
                string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (guid != inputReport.Id) return BadRequest();
                ViolationReport? beforeUpdateObj = await _ReportRepository.GetById(inputReport.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Product that need to update does not exist");
                if (beforeUpdateObj.Status != null
                    &&
                    (beforeUpdateObj.Status == (int)ReportStatusEnum.Rejected
                    || beforeUpdateObj.Status == (int)ReportStatusEnum.Censored))
                    return BadRequest("This report is done.Can not change to other status");
                ViolationReport updateObj = new ViolationReport()
                {
                    Title = beforeUpdateObj.Title,
                    Content = beforeUpdateObj.Content,
                    StaffId = inputReport.Status == (int)ReportStatusEnum.Censored || inputReport.Status == (int)ReportStatusEnum.Rejected ? uid : "",
                    Status = inputReport.Status == null ? beforeUpdateObj.Status : inputReport.Status,
                    ReporterId = beforeUpdateObj.ReporterId,
                    SubmittedDate = beforeUpdateObj.SubmittedDate,
                    ItemType = beforeUpdateObj.ItemType,
                    Id = beforeUpdateObj.Id,
                    ItemId = beforeUpdateObj.Id,
                    ReportCategoryId = beforeUpdateObj.ReportCategoryId,
                };
                await _ReportRepository.Update(updateObj);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_ReportRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }
    }
}
