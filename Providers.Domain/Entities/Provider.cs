using System;

namespace Providers.Domain.Entities
{
    public class Provider
    {
        public int id { get; set; }

        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;

        public string? email { get; set; }
        public string? phone { get; set; }

        public bool is_deleted { get; set; } = false;

        // Auditoría
        public int created_by { get; set; }
        public DateTime created_at { get; set; }

        public int updated_by { get; set; }
        public DateTime updated_at { get; set; }

        public Provider() { }

        public Provider(
            int id,
            string first_name,
            string last_name,
            string? email,
            string? phone,
            bool is_deleted,
            int created_by,
            DateTime created_at,
            int updated_by,
            DateTime updated_at)
        {
            this.id = id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.email = email;
            this.phone = phone;
            this.is_deleted = is_deleted;
            this.created_by = created_by;
            this.created_at = created_at;
            this.updated_by = updated_by;
            this.updated_at = updated_at;
        }

        public Provider(int id, string first_name, string last_name, string? email, string phone, bool is_deleted)
        {
            this.id = id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.email = email;
            this.phone = phone;
            this.is_deleted = is_deleted;
        }
    }
}