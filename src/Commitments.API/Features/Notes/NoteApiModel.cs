using Commitments.API.Features.Tags;
using Commitments.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Commitments.API.Features.Notes
{
    public class NoteApiModel
    {        
        public int NoteId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public ICollection<TagApiModel> Tags = new HashSet<TagApiModel>();

        public static NoteApiModel FromNote(Note note, bool includeTags = true)
        {
            var model = new NoteApiModel
            {
                NoteId = note.NoteId,
                Title = note.Title,
                Slug = note.Slug,
                Body = note.Body
            };

            if (includeTags)
                model.Tags = note.NoteTags.Select(x => TagApiModel.FromTag(x.Tag)).ToList();

            return model;
        }
    }
}
