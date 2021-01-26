﻿using ELMS.WEB.Adapters.Equipment;
using ELMS.WEB.Adapters.Loan;
using ELMS.WEB.Areas.Loan.Models;
using ELMS.WEB.Enums.Equipment;
using ELMS.WEB.Managers.Equipment.Interfaces;
using ELMS.WEB.Managers.Loan.Interface;
using ELMS.WEB.Models.Base.Response;
using ELMS.WEB.Models.Loan.Response;
using ELMS.WEB.Repositories.Identity.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ELMS.WEB.Areas.Loan.Controllers
{
    [Authorize]
    [Area("Loan")]
    public class LoanController : Controller
    {
        private readonly ILoanManager __LoanManager;
        private readonly IEquipmentManager __EquipmentManager;
        private readonly IUserRepository __UserRepository;
        private readonly ILoanEquipmentManager __LoanEquipmentManager;

        public LoanController(ILoanManager loanManager, IEquipmentManager equipmentManager, IUserRepository userRepository, ILoanEquipmentManager loanEquipmentManager)
        {
            __LoanManager = loanManager ?? throw new ArgumentNullException(nameof(loanManager));
            __EquipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
            __UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            __LoanEquipmentManager = loanEquipmentManager ?? throw new ArgumentNullException(nameof(loanEquipmentManager));
        }

        public async Task<IActionResult> Index(string errorMessage = "", string successMessage = "")
        {
            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                ViewData["ErrorMessage"] = errorMessage;
            }

            if (!String.IsNullOrWhiteSpace(successMessage))
            {
                ViewData["SuccessMessage"] = successMessage;
            }

            IndexViewModel _Model = new IndexViewModel();

            foreach (LoanResponse loan in await __LoanManager.GetAsync())
            {
                LoanViewModel _LoanViewModel = loan.ToViewModel();

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

            return View("Index", _Model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateViewAsync()
        {
            CreateLoanViewModel _Model = new CreateLoanViewModel
            {
                EquipmentSelectList = (await __EquipmentManager.GetAsync()).Equipments.Where(x => x.Status == Status.Available).ToList().ToViewModel(),
                UserSelectList = await __UserRepository.GetAsync()
            };

            return View("CreateLoan", _Model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateLoanViewModel model)
        {
            model.LoanerUID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.FromTimestamp >= model.ExpiryTimestamp)
            {
                ModelState.AddModelError("Error", "From Date must be less than the Expiry Date");
            }

            if (!ModelState.IsValid)
            {
                model.EquipmentSelectList = (await __EquipmentManager.GetAsync()).Equipments.Where(x => x.Status == Status.Available).ToList().ToViewModel();
                model.UserSelectList = await __UserRepository.GetAsync();

                ViewData["ErrorMessage"] = "Invalid form submission";
                return View("CreateLoan", model);
            }

            model.LoanerUID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            LoanResponse _Response = await __LoanManager.CreateAsync(model.ToRequest());

            if (!_Response.Success)
            {
                ModelState.AddModelError("Error", _Response.Message);
                return View("CreateLoan", model);
            }

            return RedirectToAction("Index", "Loan", new { Area = "Loan" });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptTermsAndConditionsViewAsync(Guid loanUID)
        {
            if (loanUID == null || loanUID == Guid.Empty)
            {
                return Json("Page not found");
            }

            LoanResponse _Response = await __LoanManager.GetByUIDAsync(loanUID);

            if (_Response == null)
            {
                return Json("Page not found");
            }

            LoanViewModel _LoanViewModel = _Response.ToViewModel();

            IList<Guid> _EquipmentUIDs = (await __LoanEquipmentManager.GetAsync(_Response.UID)).Select(x => x.EquipmentUID).ToList();
            if (_EquipmentUIDs != null && _EquipmentUIDs.Count > 0)
            {
                _LoanViewModel.EquipmentList = (await __EquipmentManager.GetAsync(_EquipmentUIDs)).Equipments.ToViewModel();
            }

            if (_Response.LoaneeUID != Guid.Empty)
            {
                _LoanViewModel.Loanee = await __UserRepository.GetByUIDAsync(_Response.LoaneeUID);
            }

            if (_Response.LoanerUID != Guid.Empty)
            {
                _LoanViewModel.Loaner = await __UserRepository.GetByUIDAsync(_Response.LoanerUID);
            }

            AcceptTermsAndConditionsViewModel _Model = new AcceptTermsAndConditionsViewModel
            {
                UID = loanUID,
                Accepted = false,
                Loan = _LoanViewModel
            };

            return View("AcceptTermsAndConditions", _Model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AcceptTermsAndConditionsAsync(AcceptTermsAndConditionsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Invalid form submission";
                return View("AcceptTermsAndConditions", model);
            }

            BaseResponse _Response = await __LoanManager.AcceptTermsAndConditions(model.UID);

            if (!_Response.Success)
            {
                ModelState.AddModelError("Error", _Response.Message);
                return await AcceptTermsAndConditionsViewAsync(model.UID);
            }

            return View("AcceptedTermsAndConditions");
        }
    }
}