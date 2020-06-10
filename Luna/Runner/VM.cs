using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Runner;
using Luna.Types;
using Luna.Assets;

namespace Luna {
    static class VM {
        public static void LoadRoom(Game _assets, LRoom _room) {
            _assets.CurrentRoom = _room;
            for (int i = 0; i < _room.Instances.Count; i++) {
                LRoomInstance _instGet = _room.Instances[i];
                LInstance _instCreate = new LInstance(_assets, _instGet.Index, _instGet.X, _instGet.Y);
                _instCreate.RoomPreCreate = _instGet.PreCreate;
                _instCreate.RoomCreate = _instGet.CreationCode;
                _instCreate.Variables["image_xscale"] = new LValue(LType.Number, (double)_instGet.ScaleX);
                _instCreate.Variables["image_yscale"] = new LValue(LType.Number, (double)_instGet.ScaleY);
                _instCreate.Variables["image_speed"] = new LValue(LType.Number, (double)_instGet.ImageSpeed);
                _instCreate.Variables["image_index"] = new LValue(LType.Number, (double)_instGet.ImageIndex);
                _instCreate.Variables["image_blend"] = new LValue(LType.Number, (double)_instGet.ImageBlend);
                _instCreate.Variables["image_angle"] = new LValue(LType.Number, (double)_instGet.Rotation);
                _assets.Instances.Add((double)_instCreate.ID, _instCreate); // TODO: this is probably unneeded
                if (_instCreate.PreCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.PreCreate);
                if (_instCreate.Create != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.Create);
                if (_instCreate.RoomPreCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.RoomPreCreate);
                if (_instCreate.RoomCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.RoomCreate);
            }

            if (_room.CreationCode != null) {
                Domain _roomEnvironment = new Domain(new LInstance(-100));
                _roomEnvironment.ExecuteCode(_assets, _room.CreationCode);
            }
        }
    }
}
