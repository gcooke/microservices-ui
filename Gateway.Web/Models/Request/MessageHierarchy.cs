using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Request
{
    public class MessageHierarchy
    {
        public Guid CorrelationId { get; set; }

        public Guid ParentCorrelationId { get; set; }

        public string Controller { get; set; }

        public DateTime StartUtc { get; set; }

        public DateTime? EndUtc { get; set; }

        public string Successful { get; set; }

        public int? Size { get; set; }

        public int? QueueTimeMs { get; set; }

        public int? ProcessingTimeMs { get; set; }

        public int? TotalTimeMs { get; set; }

        public string SizeUnit { get; set; }

        public List<MessageHierarchy> ChildRequests { get; } = new List<MessageHierarchy>();

        public int StartTimeMs { get; set; }

        public decimal QueueTime { get; set; }

        public decimal ProcessingTime { get; set; }

        public decimal StartTime { get; set; }

        public long RequestSize { get; set; }

        public long ResponseSize { get; set; }
    }

    internal static class MessageHierarchyUtils
    {
        public static MessageHierarchy ToModel(this spGetRequestChildrenPayloadDetails_Result record)
        {
            return new MessageHierarchy
            {
                CorrelationId = record.CorrelationId,
                ParentCorrelationId = record.ParentCorrelationId,
                Controller = record.Controller,
                StartUtc = record.StartUtc,
                EndUtc = record.EndUtc,
                Successful = record.ResultCode.GetValueOrDefault() == 1 ? "true" : "false",
                QueueTimeMs = record.QueueTimeMs,
                ProcessingTimeMs = record.TimeTakeMs - record.QueueTimeMs,
                TotalTimeMs = record.TimeTakeMs,
                Size = record.Size,
                SizeUnit = record.SizeUnit,
                RequestSize = record.RequestLength,
                ResponseSize = record.ResponseLength
            };
        }

        public static MessageHierarchy ToTree(this List<MessageHierarchy> items, Guid rootCorrelationId)
        {
            // Convert into lookup dictionaries by parent correlation id.
            var lookup = ToHierarchicalLookup(items);

            // Find root item.
            var root = items.FirstOrDefault(i => i.CorrelationId == rootCorrelationId);

            // Populate tree.
            if (root != null)
                Populate(root, lookup);

            return root;
        }

        private static void Populate(MessageHierarchy root, IDictionary<Guid, List<MessageHierarchy>> childrenLookup)
        {
            if (!childrenLookup.TryGetValue(root.CorrelationId, out var children)) return;

            foreach (var child in children)
            {
                root.ChildRequests.Add(child);
                Populate(child, childrenLookup);
            }
        }

        private static IDictionary<Guid, List<MessageHierarchy>> ToHierarchicalLookup(List<MessageHierarchy> items)
        {
            var result = new Dictionary<Guid, List<MessageHierarchy>>();

            foreach (var item in items)
            {
                if (!result.TryGetValue(item.ParentCorrelationId, out var list))
                {
                    list = new List<MessageHierarchy>();
                    result.Add(item.ParentCorrelationId, list);
                }

                list.Add(item);
            }

            return result;
        }
    }
}