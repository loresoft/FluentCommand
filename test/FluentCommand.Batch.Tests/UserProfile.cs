using System;
using DataGenerator;
using DataGenerator.Sources;
using FluentCommand.Entities;

namespace FluentCommand.Batch.Tests
{
    public class UserProfile : MappingProfile<User>
    {
        public override void Configure()
        {
            Property(p => p.FirstName).DataSource<FirstNameSource>();
            Property(p => p.LastName).DataSource<LastNameSource>();
            Property(p => p.DisplayName).Value(u => $"{u.FirstName} {u.LastName}");
            Property(p => p.EmailAddress).Value(u => $"{u.FirstName}.{u.LastName}.{DateTime.Now.Ticks}@mailinator.com");
            Property(p => p.IsEmailAddressConfirmed).Value(true);
            Property(p => p.PasswordHash).DataSource<PasswordSource>();
            Property(p => p.IsDeleted).Value(false);
            Property(p => p.LockoutEnabled).Value(false);
            Property(p => p.AccessFailedCount).Value(0);


            Property(p => p.Created).Value(u => DateTimeOffset.UtcNow);
            Property(p => p.CreatedBy).Value("UnitTest");
            Property(p => p.Updated).Value(u => DateTimeOffset.UtcNow);
            Property(p => p.UpdatedBy).Value("UnitTest");

            Property(p => p.RowVersion).Ignore();
        }
    }
}