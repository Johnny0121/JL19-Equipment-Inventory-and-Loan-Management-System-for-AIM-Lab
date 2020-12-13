﻿using ELMS.WEB.Entities.Equipment;
using ELMS.WEB.Models;
using ELMS.WEB.Repositories.Equipment.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELMS.WEB.Repositories.Equipment.Concrete
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly ApplicationContext __Context;

        public EquipmentRepository(ApplicationContext context)
        {
            __Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<EquipmentEntity> CreateAsync(EquipmentEntity equipment)
        {
            if (equipment == null || equipment.UID == Guid.Empty)
            {
                return null;
            }

            await __Context.Equipment.AddAsync(equipment);
            bool _Added = await __Context.SaveChangesAsync() > 0;

            return _Added ? equipment : null;
        }

        public async Task<bool> DeleteAsync(Guid uid)
        {
            if (uid == Guid.Empty)
            {
                return false;
            }

            EquipmentEntity _Equipment = await __Context.Equipment.FirstOrDefaultAsync(x => x.UID == uid);

            if (_Equipment == null)
            {
                return false;
            }

            __Context.Equipment.Remove(_Equipment);

            return await __Context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<EquipmentEntity>> GetAsync()
        {
            return await __Context.Equipment.ToListAsync();
        }

        public async Task<EquipmentEntity> GetAsync(Guid uid)
        {
            return await __Context.Equipment.FirstOrDefaultAsync(x => x.UID == uid);
        }

        public async Task<bool> UpdateAsync(EquipmentEntity equipment)
        {
            if (equipment.UID == Guid.Empty)
            {
                return false;
            }

            EquipmentEntity _Equipment = await __Context.Equipment.FirstOrDefaultAsync(x => x.UID == equipment.UID);

            if (_Equipment == null)
            {
                return false;
            }

            _Equipment.Name = equipment.Name;
            _Equipment.Description = equipment.Description;
            _Equipment.SerialNumber = equipment.SerialNumber;
            _Equipment.WarrantyExpirationDate = equipment.WarrantyExpirationDate;
            _Equipment.PurchaseDate = equipment.PurchaseDate;
            _Equipment.PurchasePrice = equipment.PurchasePrice;
            _Equipment.Status = equipment.Status;

            return await __Context.SaveChangesAsync() > 0;
        }
    }
}