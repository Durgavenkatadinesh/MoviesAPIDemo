using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MoviesAPIDemo.Data;
using MoviesAPIDemo.Entities;
using MoviesAPIDemo.Models;
using System.Net.Http.Headers;
using System.Reflection;

namespace MoviesAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        CreateMovieViewModel model = new CreateMovieViewModel();
        private readonly MovieDBContext _context;

        public MovieController(MovieDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex=0,int pageSize=10)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _context.Movie.Include(x => x.Persons).Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x=>new MovieListViewModel
                    {
                        Id=x.Id,
                        Title=x.Title,
                        Actors=x.Persons.Select(y=>new ActorViewModel
                        {
                            Id=y.Id,
                            Name=y.Name,
                            DateOfBirth=y.DateOfBirth,
                        }).ToList(),
                        CoverImage=x.CoverImage,
                        Language=x.Language,
                        ReleaseDate=x.ReleaseDate,
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data=new {Movies=movieList,Count=movieCount};

                return Ok(response);
            }
            catch (Exception ex) {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _context.Movie.Include(x => x.Persons).Where(x => x.Id == id).
                    Select(x => new MovieDetailsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Actors = x.Persons.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = x.CoverImage,
                        Language = x.Language,
                        ReleaseDate = x.ReleaseDate,
                        Description = x.Description,
                    }).FirstOrDefault();


                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Record Not Exist.";
                    return BadRequest(response) ;
                }
                response.Status = true;
                response.Message = "Success";
                response.Data = movie;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned.";
                        return BadRequest(response);
                    }

                    var postedModel = new Movie()
                    {
                        Title = model.Title,
                        ReleaseDate = model.ReleaseDate,
                        Language = model.Language,
                        CoverImage = model.CoverImage,
                        Description = model.Description,
                        Persons = actors
                    };

                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responseData = new MovieDetailsViewModel
                    {
                        Id = postedModel.Id,
                        Title = postedModel.Title,
                        Actors = postedModel.Persons.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = postedModel.CoverImage,
                        Language = postedModel.Language,
                        ReleaseDate = postedModel.ReleaseDate,
                        Description = postedModel.Description,
                    };

                    response.Status = true;
                    response.Message = "Created Successfully.";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation Failed.";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid Movie Record.";
                        return BadRequest(response);
                    }

                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned.";
                        return BadRequest(response);
                    }

                    var movieDetails = _context.Movie.Include(x => x.Persons).Where(x => x.Id == model.Id).FirstOrDefault();

                    if (movieDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid Movie Record.";
                        return BadRequest(response);
                    }

                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.Description = model.Description;
                    movieDetails.Language = model.Language;
                    movieDetails.ReleaseDate = model.ReleaseDate;
                    movieDetails.Title = model.Title;

                    // Update actors in the movie
                    var removedActors = movieDetails.Persons.Where(x => !model.Actors.Contains(x.Id)).ToList();
                    foreach (var actor in removedActors)
                    {
                        movieDetails.Persons.Remove(actor);
                    }
                    //find added actors
                    var addedActors = actors.Except(movieDetails.Persons).ToList();
                    foreach (var actor in addedActors)
                    {
                        movieDetails.Persons.Add(actor);
                    }

                    _context.SaveChanges();

                    var responseData = new MovieDetailsViewModel
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        Actors = movieDetails.Persons.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = movieDetails.CoverImage,
                        Language = movieDetails.Language,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Description = movieDetails.Description,
                    };

                    response.Status = true;
                    response.Message = "Updated Successfully.";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation Failed.";
                    response.Data = ModelState;
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Where(x => x.Id == id).FirstOrDefault();
                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Invalid Movie Record";

                    return BadRequest(response);
                }

                _context.Movie.Remove(movie);
                _context.SaveChanges();

                response.Status = true;
                response.Message = "Deleted Successfully";

                return Ok(response);
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        /*[HttpPost]
        [Route("upload-movie-poster")]
        public IActionResult UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                string newPath=
            }
        }*/
       


    }
}
