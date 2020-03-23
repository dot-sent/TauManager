using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PlayerHistory
    {
        public PlayerHistory() {}
        public PlayerHistory(Player p)
        {
            Player = p;
            PlayerId = p.Id;
            RecordedAt = p.LastUpdate;
            Level = p.Level;
            Strength = p.Strength;
            Stamina = p.Stamina;
            Agility = p.Agility;
            Intelligence = p.Intelligence;
            Social = p.Social;
            Wallet = p.Wallet;
            Bank = p.Bank;
            Bonds = p.Bonds;
        }
        public long Id { get; set; }
        public virtual Player Player { get; set; }
        public int PlayerId { get; set; }
        public DateTime RecordedAt { get; set; }
        public decimal Level { get; set; }
        public decimal Strength { get; set; }
        public decimal Stamina { get; set; }
        public decimal Agility { get; set; }
        public decimal Intelligence { get; set; }
        public decimal Social { get; set; }
        public decimal Wallet { get; set; }
        public decimal Bank { get; set; }
        public int Bonds { get; set; }    
        public bool IsDifferent(Player player)
        {
            if (player.LastUpdate == this.RecordedAt) return false;
            return (
                (Math.Abs(player.Strength - this.Strength) + 
                Math.Abs(player.Stamina - this.Stamina) + 
                Math.Abs(player.Agility - this.Agility) + 
                Math.Abs(player.Intelligence - this.Intelligence) +
                Math.Abs(player.Social - this.Social) + 
                Math.Abs(player.Bank - this.Bank) + 
                Math.Abs(player.Level - this.Level) + 
                Math.Abs(player.Bonds - this.Bonds)) > (decimal)0.001
            );
        }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal StatTotal 
        {
            get
            {
                return Strength + Stamina + Agility;
            }
        }
    }
}