﻿using MoviesAPIDemo.Entities;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPIDemo.Models
{
    public class CreateMovieViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage="Name of the movie is required.")]
        public string Title { get; set; }

        public string Description { get; set; }

        public List<int> Actors { get; set; }

        [Required(ErrorMessage = "Language of the movie is required.")]
        public string Language { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string CoverImage { get; set; }

    }
}
