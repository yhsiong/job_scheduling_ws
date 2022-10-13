﻿using Job_Scheduling.Model;
using Microsoft.EntityFrameworkCore;

namespace Job_Scheduling.Database
{
    public class Schedule_Context : DbContext
    {
        public Schedule_Context(DbContextOptions<Schedule_Context> options) : base(options)
        {

        }

        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Schedule_Job> Schedule_Job { get; set; }
    }
}