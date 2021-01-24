﻿using ELMS.WEB.Adapters.Equipment;
using ELMS.WEB.Areas.Equipment.Models;
using ELMS.WEB.Helpers;
using ELMS.WEB.Managers.Equipment.Interfaces;
using ELMS.WEB.Models.Base.Response;
using ELMS.WEB.Models.Equipment.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ELMS.WEB.Areas.Equipment.Controllers
{
    [Authorize]
    [Area("Equipment")]
    public class EquipmentController : Controller
    {
        private readonly IConfiguration __Configuration;
        private readonly IEquipmentManager __EquipmentManager;
        private readonly INoteManager __NoteManager;
        private readonly String ENTITY_NAME = "Equipment";

        public EquipmentController(IConfiguration configuration, IEquipmentManager equipmentManager, INoteManager noteManager)
        {
            __Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            __EquipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
            __NoteManager = noteManager ?? throw new ArgumentNullException(nameof(noteManager));
        }

        public Task<IActionResult> SendGridSampleAsync()
        {
            //EmailSender _Sender = new EmailSender(__Configuration);
            //return Json(await _Sender.SendEmailSampleAsync());
            return null;
        }

        public async Task<IActionResult> IndexAsync(String errorMessage, String successMessage)
        {
            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                ViewData["ErrorMessage"] = errorMessage;
            }
            else if (!String.IsNullOrWhiteSpace(successMessage))
            {
                ViewData["SuccessMessage"] = successMessage;
            }

            IndexViewModel _Model = new IndexViewModel
            {
                Equipment = (await __EquipmentManager.GetAsync()).Equipments.ToViewModel()
            };

            return View(_Model);
        }

        public async Task<IActionResult> CreateModalAsync()
        {
            CreateEquipmentViewModel _Model = new CreateEquipmentViewModel();
            return PartialView("_CreateEquipment", _Model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateEquipmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Invalid form submission";
                return PartialView("_CreateEquipment", model);
            }

            BaseResponse _Response = new BaseResponse();
            model.OwnerUID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.Quantity <= 1)
            {
                _Response = await __EquipmentManager.CreateAsync(model.ToRequest());
            }
            else
            {
                _Response = await __EquipmentManager.BulkCreateAsync(model.ToRequest());
            }

            if (!_Response.Success)
            {
                ModelState.AddModelError("Error", _Response.Message);
                return await CreateModalAsync();
            }

            return Json(new { success = $"{GlobalConstants.SUCCESS_ACTION_PREFIX} created {ENTITY_NAME}" });
        }

        public async Task<IActionResult> EditViewAsync(Guid equipmentUID)
        {
            EquipmentResponse _Response = await __EquipmentManager.GetAsync(equipmentUID);

            if (!_Response.Success)
            {
                return RedirectToAction("Index", new { errorMessage = _Response.Message });
            }

            return View("Edit", _Response.ToUpdateViewModel());
        }

        public async Task<IActionResult> EditAsync(DetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Invalid form submission";
            }

            BaseResponse _Response = await __EquipmentManager.UpdateAsync(model.Equipment);

            if (!_Response.Success)
            {
                ViewData["ErrorMessage"] = _Response.Message;
            }
            else
            {
                ViewData["SuccessMessage"] = _Response.Message;
            }

            return await DetailsViewAsync(model.Equipment.UID);
        }

        public async Task<IActionResult> DetailsViewAsync(Guid equipmentUID)
        {
            if (equipmentUID == null || equipmentUID == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Invalid equipment UID";
            }

            DetailsViewModel _Model = new DetailsViewModel
            {
                Equipment = (await __EquipmentManager.GetAsync(equipmentUID)).ToViewModel(),
                Notes = (await __NoteManager.GetAsync(equipmentUID)).ToViewModel()
            };

            return View("Details", _Model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteModalAsync(Guid equipmentUID)
        {
            EquipmentResponse _Response = await __EquipmentManager.GetAsync(equipmentUID);

            if (!_Response.Success)
            {
                return RedirectToAction("DetailsView", "Equipment", new { Area = "Equipment", errorMessage = _Response.Message });
            }

            return PartialView("_DeleteEquipment", _Response.ToViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(DeleteEquipmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Invalid form submission.";
                return PartialView("_DeleteStock", model);
            }

            BaseResponse _Response = await __EquipmentManager.DeleteAsync(model);

            if (!_Response.Success)
            {
                return RedirectToAction("Index", "Equipment", new { Area = "Equipment", errorMessage = _Response.Message });
            }

            return RedirectToAction("Index", "Equipment", new { Area = "Equipment", successMessage = $"{GlobalConstants.SUCCESS_ACTION_PREFIX} deleted {ENTITY_NAME}" });
        }
    }
}