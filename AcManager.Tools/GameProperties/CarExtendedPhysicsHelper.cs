using System.Linq;
using AcManager.Tools.Managers;
using AcManager.Tools.Objects;
using AcTools.Utils.Helpers;
using FirstFloor.ModernUI.Helpers;

namespace AcManager.Tools.GameProperties {
    public class CarExtendedPhysicsHelper : CarSpecificHelperBase {
        public static bool IsActive => _isActive > 0;
        private static int _isActive = 0;

        private const string KeyModifiedIds = "CarExtendedPhysicsHelper.ModifiedIds";

        public static void Revert() {
            if (!ValuesStorage.Contains(KeyModifiedIds)) return;
            foreach (var car in ValuesStorage.GetStringList(KeyModifiedIds).Select(x => CarsManager.Instance.GetById(x)).NonNull()) {
                if (car.SetExtendedPhysics(false)) {
                    Logging.Write("Original data is reverted: " + car);
                } else {
                    Logging.Warning("Failed to revert original data: " + car);
                }
            }
            ValuesStorage.Remove(KeyModifiedIds);
        }

        protected override bool SetOverride(CarObject car) {
            if (!car.SetExtendedPhysics(true)) return false;
            Logging.Write("Custom data is set: " + car);
            ValuesStorage.Storage.SetStringList(KeyModifiedIds, ValuesStorage.GetStringList(KeyModifiedIds).Append(car.Id));
            _isActive++;
            return true;
        }

        protected override void DisposeOverride() {
            _isActive--;
            Revert();
        }
    }
}