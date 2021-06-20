﻿using System;
using System.Linq;
using AcTools.ExtraKn5Utils.Kn5Utils;
using AcTools.Kn5File;
using AcTools.Render.Base;
using AcTools.Render.Base.Cameras;
using AcTools.Render.Base.Objects;
using AcTools.Utils.Helpers;
using SlimDX;

namespace AcTools.Render.Kn5Specific.Objects {
    public sealed class Kn5RenderableList : RenderableList {
        public readonly Kn5Node OriginalNode;

        private readonly string _dirNode;
        private bool _dirTargetSet;
        private RenderableList _dirTarget;

        public Kn5RenderableList(Kn5Node node, IKn5ToRenderableConverter converter)
                : base(node.Name, node.Transform.ToMatrix(),
                        node.Children.Count == 0 ? new IRenderableObject[0] : node.Children.Select(converter.Convert).NonNull()) {
            OriginalNode = node;
            if (IsEnabled && (!OriginalNode.Active || OriginalNode.Name == "CINTURE_ON" || OriginalNode.Name.StartsWith("DAMAGE_GLASS"))) {
                IsEnabled = false;
            }

            if (node.Name.StartsWith("DIR_")) {
                _dirNode = node.Name.Substring(4);
            }
        }

        public override void Draw(IDeviceContextHolder holder, ICamera camera, SpecialRenderMode mode, Func<IRenderableObject, bool> filter = null) {
            if (_dirNode != null && !_dirTargetSet) {
                _dirTargetSet = true;

                var model = holder.TryToGet<IKn5Model>();
                if (model != null) {
                    _dirTarget = model.GetDummyByName(_dirNode);
                    _dirTarget?.LookAt(this);
                }
            }

            base.Draw(holder, camera, mode, filter);
        }

        public Matrix ModelMatrixInverted { get; internal set; }
        public Matrix RelativeToModel => Matrix * ModelMatrixInverted;
    }
}