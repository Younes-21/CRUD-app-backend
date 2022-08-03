using DEBUT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace DEBUT.Controllers
{[RoutePrefix("api/test")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TestController : ApiController

    {
        private SGBD db = new SGBD();
        [HttpGet]
        [Route("")]
        public IHttpActionResult Select()
        {
            return Ok(db.Cmd("exec getbrand"));
        }
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult Selectid(int id)
        {
            return Ok(
                db.Cmd("exec getbrandbyid @id", new Dictionary<string, object> { {"id", id} })

                );
        }
        [HttpDelete]
        [Route("delete/{id}")]
        public IHttpActionResult Delete(int id)
        {
            return Ok(
                db.Cmd("exec deletebrandbyid @id", new Dictionary<string, object> { {"id", id} })

                );
        }

        [HttpPut]
        [Route("update/{id}/{name}/{desc}/{image}")]
        public IHttpActionResult Update(int id,string name,string desc,string image)
        {
            return Ok(
                
                db.Cmd("exec updatebrandbyid @id , @name , @desc , @image", new Dictionary<string, object> { { "id", id },{"name",name }, { "desc", desc } , { "image", image } })

                ) ;
        }

        [HttpPost]
        [Route("insert/{name}/{desc}/{image}")]
        public IHttpActionResult Insert(string name, string desc,string image)
        {
          
                 return Ok( 
              db.Cmd("exec insertbrand @name, @desc, @image", new Dictionary<string, object> { {"name",name },{"desc",desc },{"image", image } })

               ) ;
        }
    }
}
