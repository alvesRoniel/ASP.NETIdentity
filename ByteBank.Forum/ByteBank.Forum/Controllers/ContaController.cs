﻿using ByteBank.Forum.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {

        // GET: Conta
        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(ContaRegistrarViewModel modelo)
        {
            if (modelo is null)
            {
                throw new ArgumentNullException(nameof(modelo));
            }

            return View();
        }
    }
}