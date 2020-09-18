using System;
using System.Collections.Generic;
using Bard.Infrastructure;
using Bard.Internal.Then;
using Bard.Internal.When;
using Grpc.Core;

namespace Bard.gRPC.Internal
{
    internal class When : Bard.Internal.When.When, IWhen 
    {
        private readonly GrpcClientFactory _grpcClientFactory;
        private readonly EventAggregator _eventAggregator;

        internal When(GrpcClientFactory grpcClientFactory, EventAggregator eventAggregator, Api api,
            LogWriter logWriter, Action preApiCall) : base(
            api, eventAggregator, logWriter, preApiCall)
        {
            _grpcClientFactory = grpcClientFactory;
            _eventAggregator = eventAggregator;
        }

        public TResponse Grpc<TGrpcClient, TResponse>(Func<TGrpcClient, TResponse> grpcCall) where TGrpcClient : ClientBase<TGrpcClient>
        {
            PreApiCall?.Invoke();
        
            WriteHeader();
        
            var gRpcClient = _grpcClientFactory.Create<TGrpcClient>();
        
            var response = grpcCall(gRpcClient);
        
            _eventAggregator.PublishGrpcResponse(new GrpcResponse(response));
            
            return response;
        }

        public void GrpcAll<TGrpcClient, TResponse>(List<Func<TGrpcClient, TResponse>> grpcCalls) where TGrpcClient : ClientBase<TGrpcClient>
        {
            PreApiCall?.Invoke();

            WriteHeader();

            foreach (var grpcCall in grpcCalls)
            {
                var gRpcClient = _grpcClientFactory.Create<TGrpcClient>();

                var response = grpcCall(gRpcClient);

                _eventAggregator.PublishGrpcResponse(new GrpcResponse(response));
            }
        }
    }
}