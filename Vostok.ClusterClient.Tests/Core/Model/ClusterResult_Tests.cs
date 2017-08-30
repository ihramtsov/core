﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Vostok.Clusterclient.Model;
using Xunit;

namespace Vostok.Clusterclient.Core.Model
{
    public class ClusterResult_Tests
    {
        private readonly Request request;

        public ClusterResult_Tests()
        {
            request = Request.Get("foo/bar");
        }

        [Fact]
        public void Response_property_should_return_selected_response_if_provided_with_one()
        {
            var response = new Response(ResponseCode.Ok);

            var result = new ClusterResult(ClusterResultStatus.Success, new List<ReplicaResult>(), response, request);

            result.Response.Should().BeSameAs(response);
        }

        [Fact]
        public void Response_property_should_return_timeout_response_for_time_expired_status_if_not_provided_with_selected_one()
        {
            var result = new ClusterResult(ClusterResultStatus.TimeExpired, new List<ReplicaResult>(), null, request);

            result.Response.Code.Should().Be(ResponseCode.RequestTimeout);
        }

        [Fact]
        public void Response_property_should_return_unknown_failure_response_for_unexpected_exception_status_if_not_provided_with_selected_one()
        {
            var result = new ClusterResult(ClusterResultStatus.UnexpectedException, new List<ReplicaResult>(), null, request);

            result.Response.Code.Should().Be(ResponseCode.UnknownFailure);
        }

        [Fact]
        public void Response_property_should_return_canceled_response_for_canceled_status_if_not_provided_with_selected_one()
        {
            var result = new ClusterResult(ClusterResultStatus.Canceled, new List<ReplicaResult>(), null, request);

            result.Response.Code.Should().Be(ResponseCode.Canceled);
        }

        [Fact]
        public void Throttled_factory_method_should_return_correct_result()
        {
            var result = ClusterResult.Throttled(request);

            result.Status.Should().Be(ClusterResultStatus.Throttled);
            result.Request.Should().BeSameAs(request);
            result.ReplicaResults.Should().BeEmpty();
            result.Response.Code.Should().Be(ResponseCode.TooManyRequests);
        }

        [Theory]
        [InlineData(ClusterResultStatus.Success)]
        [InlineData(ClusterResultStatus.ReplicasNotFound)]
        [InlineData(ClusterResultStatus.ReplicasExhausted)]
        [InlineData(ClusterResultStatus.IncorrectArguments)]
        public void Response_property_should_return_unknown_response_for_given_status_if_not_provided_with_selected_one(ClusterResultStatus status)
        {
            var result = new ClusterResult(status, new List<ReplicaResult>(), null, request);

            result.Response.Code.Should().Be(ResponseCode.Unknown);
        }

        [Fact]
        public void Replica_property_should_return_address_of_replica_which_returned_final_response()
        {
            var replicaResults = new List<ReplicaResult>
            {
                new ReplicaResult(new Uri("http://replica-1"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero),
                new ReplicaResult(new Uri("http://replica-2"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero),
                new ReplicaResult(new Uri("http://replica-3"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero)
            };

            var result = new ClusterResult(ClusterResultStatus.ReplicasExhausted, replicaResults, replicaResults[1].Response, request);

            result.Replica.Should().BeSameAs(replicaResults[1].Replica);
        }

        [Fact]
        public void Replica_property_should_return_null_when_final_response_does_not_belong_to_any_replica_result()
        {
            var replicaResults = new List<ReplicaResult>
            {
                new ReplicaResult(new Uri("http://replica-1"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero),
                new ReplicaResult(new Uri("http://replica-2"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero),
                new ReplicaResult(new Uri("http://replica-3"), new Response(ResponseCode.ServiceUnavailable), ResponseVerdict.Reject, TimeSpan.Zero)
            };

            var result = new ClusterResult(ClusterResultStatus.ReplicasExhausted, replicaResults, new Response(ResponseCode.Ok), request);

            result.Replica.Should().BeNull();
        }

        [Fact]
        public void Replica_property_should_return_null_when_there_are_no_replica_results()
        {
            var replicaResults = new List<ReplicaResult>();

            var result = new ClusterResult(ClusterResultStatus.ReplicasNotFound, replicaResults, null, request);

            result.Replica.Should().BeNull();
        }
    }
}
