using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPIDemo.Data;
using MoviesAPIDemo.Entities;
using MoviesAPIDemo.Models;

namespace MoviesAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDBContext _context;

        public PersonController(MovieDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var actorCount = _context.Person.Count();
                var actorList = _context.Person.Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x => new ActorViewModel
                    {
                        Id = x.Id,
                        Name= x.Name,
                        DateOfBirth = x.DateOfBirth,
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Person = actorList, Count = actorCount };

                return Ok(response);
            }
            catch (Exception ex)
            {
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
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();


                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record Not Exist.";
                    return BadRequest(response);
                }

                var personData = new ActorDetailsViewModel
                {
                    Id = person.Id,
                    DateOfBirth = person.DateOfBirth,
                    Name = person.Name,
                    Movies = _context.Movie.Where(x => x.Persons.Contains(person)).Select(x => x.Title).ToArray()
                };

                response.Status = true;
                response.Message = "Success";
                response.Data = personData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("Search/{searchText}")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var searchedPerson = _context.Person
                    .Where(x => x.Name.Contains(searchText))
                    .Select(x => new {
                    x.Id,
                    x.Name,
                }).ToList();

                if (searchedPerson.Any())
                {
                    response.Status = true;
                    response.Message = "Success";
                    response.Data = searchedPerson;
                }
                else
                {
                    response.Status = false;
                    response.Message = "No Records Found";
                    response.Data = null;
                }

                return Ok(response);
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {

                    var postedModel = new Person()
                    {
                        Name = model.Name,
                        DateOfBirth= model.DateOfBirth
                    };

                    _context.Person.Add(postedModel);
                    _context.SaveChanges();

                    model.Id = postedModel.Id;

                    response.Status = true;
                    response.Message = "Created Successfully.";
                    response.Data = model;

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
        public IActionResult Put(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    //var postedModel = _mapper.Map<Person>(model);
                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid person record";

                        return BadRequest(response);
                    }

                    var personDetails = _context.Person.Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefault();
                    if (personDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid person record";

                        return BadRequest(response);
                    }

                    personDetails.Name = model.Name;
                    personDetails.DateOfBirth=model.DateOfBirth;

                    _context.Person.Update(personDetails);
                    _context.SaveChanges();

                    response.Status = true;
                    response.Message = "Updated Successfully.";
                    response.Data = personDetails;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch(Exception ex)
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
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();
                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Invalid Person Record";

                    return BadRequest(response);
                }

                _context.Person.Remove(person);
                _context.SaveChanges();

                response.Status = true;
                response.Message = "Deleted Successfully";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }
    }
}
