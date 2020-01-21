﻿using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Common;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.EventSourcing.Client;

namespace org.neurul.Cortex.Application.Neurons
{
    public class NeuronQueryService : INeuronQueryService
    {
        private readonly IEventSourceFactory eventSourceFactory;
        private readonly ISettingsService settingsService;

        public NeuronQueryService(IEventSourceFactory eventSourceFactory, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(eventSourceFactory, nameof(eventSourceFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.eventSourceFactory = eventSourceFactory;
            this.settingsService = settingsService;
        }

        public async Task<NeuronData> GetNeuronById(string avatarId, Guid id, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotEmpty(avatarId, "Specified parameter cannot be null or empty.", nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );

            // Using a random Guid for Author as we won't be saving anyway
            var eventSource = this.eventSourceFactory.Create(
                Helper.GetAvatarUrl(this.settingsService.EventSourcingInBaseUrl, avatarId),
                Helper.GetAvatarUrl(this.settingsService.EventSourcingOutBaseUrl, avatarId),
                Guid.NewGuid()
                );

            var neuron = await eventSource.Session.Get<Neuron>(id, cancellationToken: token);

            return new NeuronData()
            {
                Id = neuron.Id.ToString(),
                Active = neuron.Active,
                Version = neuron.Version
            };
        }
    }
}