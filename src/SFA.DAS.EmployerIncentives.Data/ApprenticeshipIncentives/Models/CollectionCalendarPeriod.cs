﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.CollectionCalendar")]
    [Table("CollectionCalendar", Schema = "incentives")]
    public partial class CollectionCalendarPeriod
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public int Id { get; set; }
        public byte PeriodNumber { get; set; }
        public byte CalendarMonth { get; set; }
        public short CalendarYear { get; set; }
        public DateTime EIScheduledOpenDateUTC { get; set; }
        public DateTime CensusDate { get; set; }
        public string AcademicYear { get; set; }
        public bool Active { get; set; }
    }
}
