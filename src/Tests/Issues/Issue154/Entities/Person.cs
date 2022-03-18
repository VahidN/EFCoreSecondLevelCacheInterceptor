using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Issue154.Entities
{
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}