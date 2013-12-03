﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace NuGet.Services.Jobs
{
    public class JobDispatcherFacts
    {
        public class TheDispatchMethod
        {
            [Fact]
            public async Task GivenNoJobWithName_ItThrowsUnknownJobException()
            {
                // Arrange
                var dispatcher = new JobDispatcher(ServiceConfiguration.Create(), Enumerable.Empty<JobDescription>(), monitor: null);
                var request = new JobRequest("flarg", "test", new Dictionary<string, string>());
                var invocation = new JobInvocation(Guid.NewGuid(), request, DateTimeOffset.UtcNow);
                var context = new InvocationContext(invocation, config: null, monitoring: null, queue: null);

                // Act/Assert
                var ex = await AssertEx.Throws<UnknownJobException>(() => dispatcher.Dispatch(context));
                Assert.Equal("flarg", ex.JobName);
            }

            [Fact]
            public async Task GivenJobWithName_ItCreatesAnInvocationAndInvokesJob()
            {
                // Arrange
                var jobImpl = new Mock<JobBase>();
                var job = new JobDescription("test", "blarg", () => jobImpl.Object);

                var dispatcher = new JobDispatcher(ServiceConfiguration.Create(), new[] { job }, monitor: null);
                var request = new JobRequest("Test", "test", new Dictionary<string, string>());
                var invocation = new JobInvocation(Guid.NewGuid(), request, DateTimeOffset.UtcNow);
                var context = new InvocationContext(invocation, config: null, monitoring: null, queue: null);

                jobImpl.Setup(j => j.Invoke(It.IsAny<InvocationContext>()))
                   .Returns(Task.FromResult(InvocationResult.Completed()));


                // Act
                var response = await dispatcher.Dispatch(context);

                // Assert
                Assert.Same(invocation, response.Invocation);
                Assert.Equal(InvocationStatus.Completed, response.Result.Status);
            }

            [Fact]
            public async Task GivenJobWithName_ItReturnsResponseContainingInvocationAndResult()
            {
                // Arrange
                var jobImpl = new Mock<JobBase>();
                var job = new JobDescription("test", "blarg", () => jobImpl.Object);
                
                var ex = new Exception();
                var dispatcher = new JobDispatcher(ServiceConfiguration.Create(), new[] { job }, monitor: null);
                var request = new JobRequest("Test", "test", new Dictionary<string, string>());
                var invocation = new JobInvocation(Guid.NewGuid(), request, DateTimeOffset.UtcNow);
                var context = new InvocationContext(invocation, config: null, monitoring: null, queue: null);

                jobImpl.Setup(j => j.Invoke(It.IsAny<InvocationContext>()))
                   .Returns(Task.FromResult(InvocationResult.Completed()));

                // Act
                var response = await dispatcher.Dispatch(context);

                // Assert
                Assert.Same(invocation, response.Invocation);
                Assert.Equal(InvocationStatus.Completed, response.Result.Status);
                Assert.Null(response.Result.Exception);
            }
        }
    }
}
