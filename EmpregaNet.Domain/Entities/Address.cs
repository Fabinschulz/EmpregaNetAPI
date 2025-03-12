using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um endereço
    /// </summary>
    public class Address
    {
        public required string AddressName { get; set; }

        public required string ZipCode { get; set; }

        public required string City { get; set; }

        public string State
        {
            get
            {
                if (UF == UF.NaoSelecionado)
                {
                    return "";
                }
                return UF.ToString();
            }
        }

        public UF UF { get; set; }

        public required string District { get; set; }

        public required string Number { get; set; }

        public required string Complement { get; set; }

    }
}