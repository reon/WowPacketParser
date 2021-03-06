using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V6_0_2_19033.Parsers
{
    public static class ItemHandler
    {
        public static int ReadItemInstance(Packet packet, params object[] indexes)
        {

            var itemId = packet.ReadEntry<Int32>(StoreNameType.Item, "ItemID", indexes);
            packet.ReadUInt32("RandomPropertiesSeed", indexes);
            packet.ReadUInt32("RandomPropertiesID", indexes);

            packet.ResetBitReader();

            var hasBonuses = packet.ReadBit("HasItemBonus", indexes);
            var hasModifications = packet.ReadBit("HasModifications", indexes);
            if (hasBonuses)
            {
                packet.ReadByte("Context", indexes);

                var indexString = Packet.GetIndexString(indexes);
                var bonusCount = packet.ReadUInt32();
                for (var j = 0; j < bonusCount; ++j)
                    packet.ReadUInt32(String.Format("{0} [{1}] BonusListID", indexString, j));
            }

            if (hasModifications)
            {
                var mask = packet.ReadUInt32();
                for (var j = 1; j <= 8; ++j)
                    if ((mask & (1u << (j - 1))) != 0)
                        packet.ReadInt32(((ItemModifier)j).ToString(), indexes);
            }

            return itemId;
        }

        [Parser(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE)]
        public static void HandleItemEnchantTimeUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("Item Guid");
            packet.ReadUInt32("Duration");
            packet.ReadUInt32("Slot");
            packet.ReadPackedGuid128("Player Guid");
        }

        [Parser(Opcode.CMSG_ITEM_REFUND_INFO)]
        public static void HandleItemRefundInfo(Packet packet)
        {
            packet.ReadPackedGuid128("Item Guid");
        }

        [Parser(Opcode.SMSG_SET_PROFICIENCY)]
        public static void HandleSetProficency(Packet packet)
        {
            packet.ReadEnum<UnknownFlags>("ProficiencyMask", TypeCode.UInt32);
            packet.ReadEnum<ItemClass>("ProficiencyClass", TypeCode.Byte);
        }

        [Parser(Opcode.CMSG_TRANSMOGRIFY_ITEMS)]
        public static void HandleTransmogrifyItems(Packet packet)
        {
            var int16 = packet.ReadInt32("ItemsCount");
            packet.ReadPackedGuid128("Npc");

            for (int i = 0; i < int16; i++)
            {
                packet.ResetBitReader();

                var bit16 = packet.ReadBit("HasSrcItem", i);
                var bit40 = packet.ReadBit("HasSrcVoidItem", i);

                ReadItemInstance(packet, i);

                packet.ReadInt32("Slot", i);

                if (bit16)
                    packet.ReadPackedGuid128("SrcItemGUID", i);

                if (bit40)
                    packet.ReadPackedGuid128("SrcVoidItemGUID", i);
            }
        }

        [Parser(Opcode.CMSG_SELL_ITEM)]
        public static void HandleSellItem(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadPackedGuid128("ItemGUID");

            packet.ReadUInt32("Amount");
        }

        [Parser(Opcode.SMSG_BUY_ITEM)]
        public static void HandleBuyItemResponse(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadUInt32("Muid");
            packet.ReadUInt32("NewQuantity");
            packet.ReadUInt32("QuantityBought");
        }

        [Parser(Opcode.SMSG_BUY_FAILED)]
        public static void HandleBuyFailed(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadEntry<UInt32>(StoreNameType.Item, "Muid");
            packet.ReadEnum<BuyResult>("Reason", TypeCode.Byte);
        }

        [Parser(Opcode.CMSG_BUY_BACK_ITEM)]
        public static void HandleBuyBackItem(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadUInt32("Slot");
        }

        [Parser(Opcode.CMSG_USE_ITEM)]
        public static void HandleUseItem(Packet packet)
        {
            packet.ReadByte("PackSlot");
            packet.ReadByte("Slot");
            packet.ReadPackedGuid128("CastItem");

            SpellHandler.ReadSpellCastRequest(ref packet);
        }

        [Parser(Opcode.CMSG_DESTROY_ITEM)]
        public static void HandleDestroyItem(Packet packet)
        {
            packet.ReadUInt32("Count");
            packet.ReadByte("ContainerId");
            packet.ReadByte("SlotNum");
        }

        [Parser(Opcode.CMSG_REPAIR_ITEM)]
        public static void HandleRepairItem(Packet packet)
        {
            packet.ReadPackedGuid128("NpcGUID");
            packet.ReadPackedGuid128("ItemGUID");

            packet.ResetBitReader();
            packet.ReadBit("UseGuildBank");
        }

        [Parser(Opcode.CMSG_AUTOSTORE_LOOT_ITEM)]
        public static void HandleAutoStoreLootItem(Packet packet)
        {
            var int16 = packet.ReadInt32("Count");

            for (var i = 0; i < int16; ++i)
            {
                packet.ReadPackedGuid128("LootObj", i);
                packet.ReadByte("Slot", i);
            }
        }

        [Parser(Opcode.CMSG_AUTOBANK_ITEM)]
        [Parser(Opcode.CMSG_AUTOEQUIP_ITEM)]
        [Parser(Opcode.CMSG_AUTOSTORE_BANK_ITEM)]
        [Parser(Opcode.CMSG_SWAP_INV_ITEM)]
        public static void HandleAutoItem(Packet packet)
        {
            var bits2 = packet.ReadBits("InvItemCount", 2);
            for (int i = 0; i < bits2; i++)
            {
                packet.ReadByte("ContainerSlot", i);
                packet.ReadByte("Slot", i);
            }

            packet.ReadByte("Slot");
            packet.ReadByte("PackSlot");
        }

        [Parser(Opcode.CMSG_SWAP_ITEM)]
        public static void HandleSwapInvItem(Packet packet)
        {
            var bits2 = packet.ReadBits("InvItemCount", 2);
            for (int i = 0; i < bits2; i++)
            {
                packet.ReadByte("ContainerSlot", i);
                packet.ReadByte("Slot", i);
            }

            packet.ReadByte("DestBag");
            packet.ReadByte("SrcBag");
            packet.ReadByte("DestSlot");
            packet.ReadByte("SrcSlot");
        }

        [Parser(Opcode.CMSG_AUTOSTORE_BAG_ITEM)]
        public static void HandleAutoBagItem(Packet packet)
        {
            var bits2 = packet.ReadBits("InvItemCount", 2);
            for (int i = 0; i < bits2; i++)
            {
                packet.ReadByte("ContainerSlot", i);
                packet.ReadByte("Slot", i);
            }

            packet.ReadByte("ContainerSlotB");
            packet.ReadByte("ContainerSlotA");
            packet.ReadByte("SlotA");
        }

        [Parser(Opcode.SMSG_COIN_REMOVED)]
        public static void HandleCoinRemoved(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
        }

        [Parser(Opcode.SMSG_INVENTORY_CHANGE_FAILURE)]
        public static void HandleInventoryChangeFailure(Packet packet)
        {
            var result = packet.ReadEnum<InventoryResult>("BagResult", TypeCode.Byte);

            for (int i = 0; i < 2; i++)
                packet.ReadPackedGuid128("Item", i);

            packet.ReadByte("ContainerBSlot");

            if (result == InventoryResult.CantEquipLevel || result == InventoryResult.PurchaseLevelTooLow)
                packet.ReadInt32("Level");
        }

        [Parser(Opcode.CMSG_SPLIT_ITEM)]
        public static void HandleSplitItem(Packet packet)
        {
            var bits2 = packet.ReadBits("InvItemCount", 2);
            for (int i = 0; i < bits2; i++)
            {
                packet.ReadByte("ContainerSlot", i);
                packet.ReadByte("Slot", i);
            }

            packet.ReadByte("SrcBag");
            packet.ReadByte("SrcSlot");
            packet.ReadByte("DestBag");
            packet.ReadByte("DestSlot");
            packet.ReadUInt32("Count");
        }

        [Parser(Opcode.CMSG_BUY_ITEM)]
        public static void HandleBuyItem(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadPackedGuid128("ContainerGUID");

            ReadItemInstance(packet);

            packet.ReadInt32("Quantity");
            packet.ReadUInt32("Muid");
            packet.ReadUInt32("Slot");

            packet.ResetBitReader();

            packet.ReadBits("ItemType", 2);
        }

        // To-Do: Need In-Game review
        [Parser(Opcode.SMSG_ITEM_PUSH_RESULT)]
        public static void HandleItemPushResult(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGUID");

            packet.ReadByte("Slot");

            packet.ReadInt32("SlotInBag");

            ReadItemInstance(packet);

            packet.ReadUInt32("WodUnk");
            packet.ReadUInt32("Quantity");
            packet.ReadUInt32("QuantityInInventory");
            packet.ReadUInt32("BattlePetBreedID");
            packet.ReadUInt32("BattlePetBreedQuality");
            packet.ReadUInt32("BattlePetSpeciesID");
            packet.ReadUInt32("BattlePetLevel");

            packet.ReadPackedGuid128("ItemGUID");

            packet.ResetBitReader();

            packet.ReadBit("Pushed");
            packet.ReadBit("DisplayText");
            packet.ReadBit("Created");
            packet.ReadBit("IsBonusRoll");
        }

        [Parser(Opcode.SMSG_SELL_RESPONSE)]
        public static void HandleSellResponse(Packet packet)
        {
            packet.ReadPackedGuid128("VendorGUID");
            packet.ReadPackedGuid128("ItemGUID");
            packet.ReadEnum<SellResult>("Reason", TypeCode.Byte);
        }

        [Parser(Opcode.SMSG_ITEM_TIME_UPDATE)]
        public static void HandleItemTimeUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("GUID");
            packet.ReadUInt32("DurationLeft");
        }
    }
}
