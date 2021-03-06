﻿using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using neurUL.Common.Http;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Domain.Model.Neurons;
using System.Threading;
using System.Threading.Tasks;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.In;

namespace neurUL.Cortex.Application.Neurons
{
    public class TerminalCommandHandlers :
        ICancellableCommandHandler<CreateTerminal>,
        ICancellableCommandHandler<DeactivateTerminal>
    {
        private readonly IEventSourceFactory eventSourceFactory;
        private readonly ISettingsService settingsService;

        public TerminalCommandHandlers(IEventSourceFactory eventSourceFactory, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(eventSourceFactory, nameof(eventSourceFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.eventSourceFactory = eventSourceFactory;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.Create(
                this.settingsService.EventSourcingInBaseUrl + "/",
                this.settingsService.EventSourcingOutBaseUrl + "/",
                message.AuthorId
                );

            Neuron presynaptic = await eventSource.Session.Get<Neuron>(message.PresynapticNeuronId, nameof(presynaptic), cancellationToken: token),
                postsynaptic = await eventSource.Session.Get<Neuron>(message.PostsynapticNeuronId, nameof(postsynaptic), cancellationToken: token);

            var terminal = new Terminal(message.Id, presynaptic, postsynaptic, message.Effect, message.Strength);
            await eventSource.Session.Add(terminal, token);
            await eventSource.Session.Commit(token);
        }

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.Create(
                this.settingsService.EventSourcingInBaseUrl + "/",
                this.settingsService.EventSourcingOutBaseUrl + "/",
                message.AuthorId
                );

            Terminal terminal = await eventSource.Session.Get<Terminal>(message.Id, nameof(terminal), message.ExpectedVersion, token);
            
            terminal.Deactivate();
            await eventSource.Session.Commit(token);
        }
    }
}
