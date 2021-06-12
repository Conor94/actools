﻿using System;
using System.Collections.Generic;
using System.Linq;
using AcManager.Tools.AcErrors;
using AcManager.Tools.AcManagersNew;
using AcManager.Tools.Managers;
using JetBrains.Annotations;

namespace AcManager.Tools.Objects {
    public sealed partial class CarObject {
        private void UpdateParentValues() {
            if (Parent == null) return;
            Parent?.OnPropertyChanged(nameof(Children));
            Parent?.OnPropertyChanged(nameof(HasChildren));
        }

        private string _parentId;

        [CanBeNull]
        public string ParentId {
            get => _parentId;
            set {
                if (string.Equals(value, Id, StringComparison.InvariantCulture)) return;
                if (value == _parentId) return;
                var oldParentId = _parentId;
                _parentId = value;

                _parentItemWrapper = null;
                if (_parentId != null) {
                    _parentSet = false;
                    var parentExists = CarsManager.Instance.CheckIfIdExists(_parentId);
                    if (parentExists) {
                        RemoveError(AcErrorType.Car_ParentIsMissing);
                    } else {
                        AddError(AcErrorType.Car_ParentIsMissing);

                        if (Loaded) {
                            UpdateParentValues();
                        }
                    }
                } else {
                    _parentSet = true;
                    RemoveError(AcErrorType.Car_ParentIsMissing);
                }

                if (HasData) {
                    CheckUpgradeIcon();
                }

                if (Loaded) {
                    OnPropertyChanged(nameof(ParentId));
                    OnPropertyChanged(nameof(Parent));
                    OnPropertyChanged(nameof(ParentDisplayName));

                    SortAffectingValueChanged();

                    if (oldParentId == null || value == null) {
                        OnPropertyChanged(nameof(IsChild));
                        OnPropertyChanged(nameof(NeedsMargin));
                    }

                    Changed = true;
                }
            }
        }

        private bool _parentSet;
        private AcItemWrapper _parentItemWrapper;

        [CanBeNull]
        public CarObject Parent {
            get {
                if (ParentId == null) return null;
                var ret = (CarObject)ParentItemWrapper?.Loaded();
                if (ret?.Outdated == true) {
                    _parentSet = false;
                    ret = (CarObject)ParentItemWrapper?.Loaded();
                }
                return ret;
            }
        }

        [CanBeNull]
        public AcItemWrapper ParentItemWrapper {
            get {
                if (ParentId == null) return null;
                if (!_parentSet) {
                    _parentSet = true;
                    _parentItemWrapper = CarsManager.Instance.GetWrapperById(ParentId);
                }
                return _parentItemWrapper;
            }
        }

        [CanBeNull]
        public string ParentDisplayName => Parent?.Name ?? ParentId;

        /* TODO: Mark as loaded only? */
        public IEnumerable<CarObject> Children => CarsManager.Instance.Loaded.Where(x => x.ParentId == Id);

        public bool HasChildren => Children.Any();

        public bool IsChild => ParentId != null;

        /* TODO: find another way, this one is way too shitty */
        public override bool NeedsMargin => Parent != null; //  && (!Parent.Enabled || Enabled);
    }
}
