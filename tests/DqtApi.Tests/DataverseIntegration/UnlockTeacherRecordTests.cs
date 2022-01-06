﻿using System;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class UnlockTeacherRecordTests : IClassFixture<CrmClientFixture>
    {
        private readonly CrmClientFixture _crmClientFixture;
        private readonly DataverseAdaptor _dataverseAdaptor;
        private readonly ServiceClient _serviceClient;

        public UnlockTeacherRecordTests(CrmClientFixture crmClientFixture)
        {
            _crmClientFixture = crmClientFixture;
            _dataverseAdaptor = crmClientFixture.CreateDataverseAdaptor();
            _serviceClient = crmClientFixture.ServiceClient;
        }

        [Fact]
        public async Task Given_an_id_that_does_not_exist_returns_false()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            // Act
            var result = await _dataverseAdaptor.UnlockTeacherRecordAsync(teacherId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(5)]
        public async Task Given_a_valid_id_sets_loginfailedcounter_to_0_and_returns_true(int? initialLoginFailedCounter)
        {
            // Arrange
            var teacherId = await _serviceClient.CreateAsync(new Contact()
            {
                dfeta_loginfailedcounter = initialLoginFailedCounter
            });
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, teacherId);

            // Act
            var result = await _dataverseAdaptor.UnlockTeacherRecordAsync(teacherId);

            // Assert
            Assert.True(result);

            var teacher = (await _serviceClient.RetrieveAsync(Contact.EntityLogicalName, teacherId, new ColumnSet() { AllColumns = true })).ToEntity<Contact>();
            Assert.Equal(0, teacher.dfeta_loginfailedcounter);
        }
    }
}