namespace RentaFacil.Bookings.Domain.Enums;

// Copia deliberada del TipoVehiculo de Vehicles.Domain (misma decisión que Periodo y Dinero,
// ver ARCHITECTURE.md §11 decisión 13): los contextos no se referencian entre sí. Los valores
// numéricos explícitos deben coincidir con los de Vehicles.Domain.Enums.TipoVehiculo, porque
// Reserva.TipoVehiculo es la copia denormalizada (§3.2) del mismo dato recibido de VehicleService.
public enum TipoVehiculo
{
    Sedan = 1,
    SUV = 2,
    Camioneta = 3,
    Van = 4,
    Pickup = 5
}
