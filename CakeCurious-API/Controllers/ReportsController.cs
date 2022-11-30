using BusinessObject;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Constants.Reports;
using Repository.Interfaces;
using Repository.Models;
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

        public ReportsController(IViolationReportRepository ReportRepository, IRecipeRepository RecipeRepository)
        {
            _ReportRepository = ReportRepository;
        }

        [HttpGet("Of-An-Item/{guid}")]
        [Authorize]
        public async Task<ActionResult<StaffReportsOfAnItemPage>> GetReportsOfAnItem(Guid? guid, string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new StaffReportsOfAnItemPage();
            result.Reports = await _ReportRepository.GetReportsOfAnItem(guid!.Value, search, sort, filter, page, size);
            result.PendingReports = await _ReportRepository.CountPendingReportOfAnItem(guid!.Value);
            result.TotalPage = (int)Math.Ceiling((decimal)_ReportRepository.CountDashboardViolationReportsOfAnItem(guid!.Value, search, filter) / size);
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
        public async Task<ActionResult<StaffReportDetail>> GetReportById(Guid guid)
        {
            var result = await _ReportRepository.GetReportDetailById(guid);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateReport(CreateReport createReport)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var report = createReport.Adapt<ViolationReport>();
                report.ReporterId = uid;
                report.SubmittedDate = DateTime.Now;
                report.Status = (int)ReportStatusEnum.Pending;
                await _ReportRepository.Add(report);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPut("bulk-update")]
        public async Task<ActionResult> UpdateReportsStatus([FromBody] BulkUpdateReportStatus reports)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (reports!.reportIds!.Count() == 0)
            {
                return BadRequest("Missing input like ids or status");
            }
            string? unUpddatedReport = await _ReportRepository.BulkUpdate(reports!.reportIds!, uid!);
            string notification = "Update reports stauts to rejected successfully.";
            if (unUpddatedReport! != null)
                notification += " Except for" + unUpddatedReport;
            return Ok(notification);
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutReport(Guid guid, ViolationReport inputReport)
        {
            try
            {
                string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (guid != inputReport.Id) return BadRequest("Input report id is different from report object id");
                ViolationReport? beforeUpdateObj = await _ReportRepository.GetById(inputReport.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Report that need to update does not exist");
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
                    ItemId = beforeUpdateObj.ItemId,
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
