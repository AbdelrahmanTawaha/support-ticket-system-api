using DataAccessLayer.ConfigurationsSetting.AiViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    internal class TicketAiReportRowConfiguration : IEntityTypeConfiguration<TicketAiReportRow>
    {
        public void Configure(EntityTypeBuilder<TicketAiReportRow> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_Tickets_AI_Report");



        }
    }
}
