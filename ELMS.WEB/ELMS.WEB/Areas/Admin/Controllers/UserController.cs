﻿using ELMS.WEB.Adapters.Equipment;
using ELMS.WEB.Adapters.Loan;
using ELMS.WEB.Areas.Admin.Data;
using NsModelPermission = ELMS.WEB.Areas.Admin.Models.Permission;
using NsModelUser = ELMS.WEB.Areas.Admin.Models.User;
using ELMS.WEB.Helpers;
using ELMS.WEB.Managers.Equipment.Interfaces;
using ELMS.WEB.Managers.Loan.Interface;
using ELMS.WEB.Models.Loan.Response;
using ELMS.WEB.Repositories.Identity.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ELMS.WEB.Entities.Admin;
using ELMS.WEB.Areas.Admin.Models.User;

namespace ELMS.WEB.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> __UserManager;
        private readonly ILoanManager __LoanManager;
        private readonly IUserRepository __UserRepository;
        private readonly ILoanEquipmentManager __LoanEquipmentManager;
        private readonly IEquipmentManager __EquipmentManager;
        private readonly string MODEL_NAME = "User";

        public UserController(UserManager<IdentityUser> userManager, ILoanManager loanManager, IUserRepository userRepository, ILoanEquipmentManager loanEquipmentManager, IEquipmentManager equipmentManager)
        {
            __UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            __LoanManager = loanManager ?? throw new ArgumentNullException(nameof(loanManager));
            __UserRepository = userRepository;
            __LoanEquipmentManager = loanEquipmentManager;
            __EquipmentManager = equipmentManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string successMessage = "", string errorMessage = "")
        {
            if (!String.IsNullOrWhiteSpace(successMessage))
            {
                ViewData["SuccessMessage"] = successMessage;
            }

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                ViewData["ErrorMessage"] = errorMessage;
            }

            NsModelUser.IndexViewModel _Model = new NsModelUser.IndexViewModel
            {
                Users = await __UserManager.Users.ToListAsync()
            };

            return View(_Model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsModalAsync(Guid uid)
        {
            IdentityUser _User = await __UserManager.FindByIdAsync(uid.ToString());

            if (_User == null)
            {
                return Json(new { success = $"{GlobalConstants.ERROR_ACTION_PREFIX} find {MODEL_NAME}" });
            }

            NsModelUser.DetailsViewModel _Model = new NsModelUser.DetailsViewModel
            {
                User = _User,
                Loans = (await __LoanManager.GetByUserAsync(_User.Email)).ToViewModel(),
                Roles = await __UserManager.GetRolesAsync(_User)
            };

            return PartialView("_DetailsModal", _Model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsViewAsync(Guid uid, string successMessage, string errorMessage)
        {

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                ViewData["SuccessMessage"] = successMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewData["ErrorMessage"] = errorMessage;
            }

            IdentityUser _User = await __UserManager.FindByIdAsync(uid.ToString());

            if (_User == null)
            {
                return RedirectToAction("Index", "User", new { Area = "Admin", errorMessage = $"User does not exist" });
            }

            NsModelUser.DetailsViewModel _Model = new NsModelUser.DetailsViewModel
            {
                User = _User,
                Roles = await __UserManager.GetRolesAsync(_User),
            };

            foreach (LoanResponse loan in await __LoanManager.GetByUserAsync(_User.Email))
            {
                Loan.Models.LoanViewModel _LoanViewModel = loan.ToViewModel();

                IList<Guid> _EquipmentUIDs = (await __LoanEquipmentManager.GetAsync(loan.UID)).Select(x => x.EquipmentUID).ToList();
                if (_EquipmentUIDs != null && _EquipmentUIDs.Count > 0)
                {
                    _LoanViewModel.EquipmentList = (await __EquipmentManager.GetAsync(_EquipmentUIDs)).Equipments.ToViewModel();
                }

                if (loan.LoaneeUID != Guid.Empty)
                {
                    _LoanViewModel.Loanee = await __UserRepository.GetByUIDAsync(loan.LoaneeUID);
                }

                if (loan.LoanerUID != Guid.Empty)
                {
                    _LoanViewModel.Loaner = await __UserRepository.GetByUIDAsync(loan.LoanerUID);
                }

                _Model.Loans.Add(_LoanViewModel);
            }

            IList<Claim> _ExistingClains = await __UserManager.GetClaimsAsync(_User);

            _Model.UserClaims = new NsModelPermission.UserClaimsViewModel
            {
                UserID = uid.ToString()
            };

            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim _UserClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                if (_ExistingClains.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    _UserClaim.IsSelected = true;
                }

                _Model.UserClaims.Claims.Add(_UserClaim);
            }

            return View("Details", _Model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserPermissionsAsync(DetailsViewModel model)
        {
            IdentityUser _User = await __UserManager.FindByIdAsync(model.UserClaims.UserID);

            if (_User == null)
            {
                return RedirectToAction("DetailsView", "User", new { Area = "Admin", uid = model.UserClaims.UserID, errorMessage = $"{GlobalConstants.ERROR_ACTION_PREFIX} find User." });
            }

            IList<Claim> _Claims = await __UserManager.GetClaimsAsync(_User);
            IdentityResult _Result = await __UserManager.RemoveClaimsAsync(_User, _Claims);

            if (!_Result.Succeeded)
            {
                return RedirectToAction("DetailsView", "User", new { Area = "Admin", uid = model.UserClaims.UserID, errorMessage = $"{GlobalConstants.ERROR_ACTION_PREFIX} update User Permissions." });
            }

            _Result = await __UserManager.AddClaimsAsync(_User,
                model.UserClaims.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));

            if (!_Result.Succeeded)
            {
                return RedirectToAction("DetailsView", "User", new { Area = "Admin", uid = model.UserClaims.UserID, errorMessage = $"{GlobalConstants.ERROR_ACTION_PREFIX} update User Permissions." });
            }

            return RedirectToAction("DetailsView", "User", new { Area = "Admin", uid = model.UserClaims.UserID, successMessage = $"{GlobalConstants.SUCCESS_ACTION_PREFIX} updated User Permissions." });
        }
    }
}