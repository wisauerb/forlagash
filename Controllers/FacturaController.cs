using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bodega.Models.Context;
using System.IO;
using Bodega.Models;
using Bodega.Helpers;
using RazorPDF;
using iTextSharp.text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Web.UI;


namespace Bodega.Controllers
{
    [Authorize(Roles="Administradores BD, Administrador de Sistema")]
    public class FacturaController : Controller
    {
        private BD_BODEGA db = new BD_BODEGA();
 
        enum FORMA
        {
            CUADRADA,
            REDONDA,
            RECTANGULAR,
            OTROS
        };

        /*Buscar el ingreso para imprimirlo*/
        public ActionResult ImprimirDetalle(int id)
        {
 
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FACTURA fACTURA = db.FACTURA.Find(id);

            if (fACTURA == null)
            {
                return HttpNotFound();
            }

            //obtener nombre del proveedor
            var proveedor = db.RAZON_SOCIAL.Where(rs => rs.RUT_PROVEEDOR == fACTURA.RUT_PROVEEDOR).OrderByDescending(rs => rs.FECHA).First();

            //enviar datos de la factura,orden de compra y proveedor
            ViewBag.CODIGO_OC = fACTURA.CODIGO_OC;
            ViewBag.FECHA_OC = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA_OC);
            ViewBag.ARCHIVO_OC = fACTURA.ARCHIVO_OC;

            ViewBag.NUMERO = fACTURA.NUMERO;
            ViewBag.FECHA = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA);
            ViewBag.TOTAL = fACTURA.TOTAL;
            ViewBag.ARCHIVO = fACTURA.ARCHIVO;

            ViewBag.NETO = fACTURA.NETO;
            ViewBag.DCTO = fACTURA.DESCUENTO;
            ViewBag.IVA = fACTURA.IVA;

            ViewBag.PROVEEDOR = proveedor.NOMBRE;
            ViewBag.RUT = proveedor.PROVEEDOR.RUT;
            ViewBag.DV = proveedor.PROVEEDOR.DV;

            ViewBag.LINEA_INVERSION = fACTURA.LINEA_INVERSION.DESC_COMPLETA;

            ViewBag.ID_INGRESO = fACTURA.ID_FACTURA;

            ViewBag.CORRELATIVO = fACTURA.CORRELATIVO;


            ViewBag.INSTITUCION = fACTURA.INSTITUCION;

            //obtener los productos asociados a la factura

            var DetalleFactura = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

            if (DetalleFactura.Count == 0)
            {
                // Si no hay productos enviar objeto vacio
                var DetalleFacturaVacia = new List<FACTURA_PRODUCTO>();
                return View(DetalleFacturaVacia);
            }

            return View(DetalleFactura);
        }

        /*Imprimir detalle de otro ingreso*/
        public ActionResult ImprimirDetalleOtros(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FACTURA fACTURA = db.FACTURA.Find(id);

            if (fACTURA == null)
            {
                return HttpNotFound();
            }

            //obtener nombre del proveedor
            var proveedor = db.RAZON_SOCIAL.Where(rs => rs.RUT_PROVEEDOR == fACTURA.RUT_PROVEEDOR).OrderByDescending(rs => rs.FECHA).First();

            //enviar datos de la factura,orden de compra y proveedor
            ViewBag.CODIGO_OC = fACTURA.CODIGO_OC;
            ViewBag.FECHA_OC = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA_OC);
            ViewBag.ARCHIVO_OC = fACTURA.ARCHIVO_OC;

            ViewBag.NUMERO = fACTURA.NUMERO;
            ViewBag.FECHA = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA);
            ViewBag.TOTAL = fACTURA.TOTAL;
            ViewBag.ARCHIVO = fACTURA.ARCHIVO;

            ViewBag.NETO = fACTURA.NETO;
            ViewBag.DCTO = fACTURA.DESCUENTO;
            ViewBag.IVA = fACTURA.IVA;

            ViewBag.PROVEEDOR = proveedor.NOMBRE;
            ViewBag.RUT = proveedor.PROVEEDOR.RUT;
            ViewBag.DV = proveedor.PROVEEDOR.DV;

            ViewBag.LINEA_INVERSION = fACTURA.LINEA_INVERSION.DESC_COMPLETA;

            ViewBag.ID_INGRESO = fACTURA.ID_FACTURA;

            ViewBag.CORRELATIVO = fACTURA.CORRELATIVO;


            ViewBag.INSTITUCION = fACTURA.INSTITUCION;

            //obtener los productos asociados a la factura

            var DetalleFactura = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

            if (DetalleFactura.Count == 0)
            {
                // Si no hay productos enviar objeto vacio
                var DetalleFacturaVacia = new List<FACTURA_PRODUCTO>();
                return View(DetalleFacturaVacia);
            }

            return View(DetalleFactura);
        }

 
        /* Listar los ingresos. Filtro con jqueryDataTable */
        public ActionResult Index()
        { 
            var ingresos = db.FACTURA.Where(i=>i.ID_ORIGEN == 1 && i.ESTADO == 1).OrderByDescending(f => f.ID_FACTURA).ToList();  
            return View(ingresos);
        }

        //muestra los ingresos eliminados
        public ActionResult Papelera()
        {
            var ingresos = db.FACTURA.Where(i => i.ID_ORIGEN == 1 && i.ESTADO == 0).OrderByDescending(f => f.ID_FACTURA).ToList();
            return View(ingresos);

        }

        // Cuando se presiona cancelar en Edit
        //@param: @id: id del ingreso
        public ActionResult Cancelar(int? id) { 
        
            //eliminar desde BD
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fACTURA = db.FACTURA.Find(id);

            if (fACTURA == null)
            {
                return HttpNotFound();
            }

            List<FACTURA_PRODUCTO> productos_asociados = new List<FACTURA_PRODUCTO>();

            /* Eliminar los productos relacionados*/
            foreach (var item in fACTURA.FACTURA_PRODUCTO)
            {
                productos_asociados.Add(item);
            }
             
            db.FACTURA.Remove(fACTURA);
             
            db.SaveChanges();

            return RedirectToAction("Index");

        }

        /**
         * Asociar atributos al producto
         *  id = id factura producto
         */
        public ActionResult NuevoAtributo(int? id) {
 
            var linea_producto =  db.FACTURA_PRODUCTO.Where(df => df.ID_FP == id).First();
            
            // ocupamos la enumeracion para obtener el listado de formas
            var formas = from FORMA s in Enum.GetValues(typeof(FORMA))
                           select new { ID = s, Name = s.ToString() };

            // seteamos la forma del producto en la lista
            ViewBag.FORMA = formas.Select(forma => 
                  new SelectListItem
                  {
                    Selected = (forma.Name == linea_producto.FORMA),
                    Text = forma.Name,
                    Value = forma.ID.ToString()
                   });

            return View("Partial/_AddAtributos", linea_producto);
        }

        public int GuardarAtributos([Bind(Include = "ID_FP, ID_FACTURA, ID_PRODUCTO, ID_TIPO_INV, CANTIDAD,CANTIDAD_RESTANTE, PRECIO_UNITARIO, PRECIO_UNITARIO_BRUTO, MARCA, MODELO, OBSERVACION, VIDA_UTIL, COLOR, MATERIALIDAD, FORMA, NUMERO_MOTOR, NUMERO_CHASIS, PATENTE,TOTAL_LINEA, TIPO_EQUIPO,TRASPASO_ACTIVO_FIJO, FECHA_RES_ALTA, NUM_RES_ALTA,CON_DOC_DIGITAL_ALTA, RUT_ADMIN_ALTA, FECHA_ADMIN_ALTA, ALTO, ANCHO, FONDO, MATERIALIDAD, COLOR, PRECIO_UNITARIO_NETO_FACT, PRECIO_UNITARIO_BRUTO_FACT, TOTAL_BRUTO_FACT,TOTAL_LINEA_FACT, TOTAL_BRUTO_CALC")] FACTURA_PRODUCTO Producto)
        {
            try
            {
                //si la cantidad restante es menor que la nueva cantidad
                int cantidadNueva = (int)Producto.CANTIDAD;
                int cantidadRestante = 0;
                int resto = 0;
                int cantidadEntregada = 0;
                
                var productoObj = db.FACTURA_PRODUCTO.Find(Producto.ID_FP);
                int cantidadActual = (int)productoObj.CANTIDAD;
                
              
                cantidadEntregada = (int)( cantidadActual - Producto.CANTIDAD_RESTANTE);
                resto = cantidadNueva - cantidadActual;

                 if(cantidadNueva > cantidadActual){ 


                     Producto.CANTIDAD_RESTANTE = (int)productoObj.CANTIDAD_RESTANTE + resto;

                 }
                 else{
                    
                     cantidadRestante = cantidadNueva - cantidadEntregada;

                     if (cantidadRestante > 0)
                     {
                         //actualizar cantidad restante
                         Producto.CANTIDAD_RESTANTE = cantidadRestante;
                     }
                     else Producto.CANTIDAD_RESTANTE = 0;
      

                     //db.Entry(Producto).State = EntityState.Modified;
                     //db.Entry(Producto).Property("CANTIDAD_RESTANTE").CurrentValue = cantidadRestante; 
                 }

                 db.Entry(productoObj).State = EntityState.Modified;
                 
                //calcular el precio unitario neto y bruto
                 
                
                productoObj.ID_FACTURA = Producto.ID_FACTURA;
                productoObj.ID_PRODUCTO = Producto.ID_PRODUCTO;
                productoObj.ID_TIPO_INV = Producto.ID_TIPO_INV;
                productoObj.PRECIO_UNITARIO = Producto.PRECIO_UNITARIO;
                productoObj.PRECIO_UNITARIO_BRUTO = Producto.PRECIO_UNITARIO_BRUTO;
                productoObj.MARCA = Producto.MARCA;
                productoObj.MODELO = Producto.MODELO;
                productoObj.OBSERVACION = Producto.OBSERVACION;
                productoObj.VIDA_UTIL = Producto.VIDA_UTIL;
                productoObj.COLOR = Producto.COLOR;
                productoObj.MATERIALIDAD = Producto.MATERIALIDAD;
                productoObj.FORMA = Producto.FORMA;
                productoObj.NUMERO_MOTOR = Producto.NUMERO_MOTOR;
                productoObj.NUMERO_CHASIS = Producto.NUMERO_CHASIS;
                productoObj.PATENTE = Producto.PATENTE;
                productoObj.TOTAL_LINEA = Producto.TOTAL_LINEA;
                productoObj.TIPO_EQUIPO = Producto.TIPO_EQUIPO;
                productoObj.TRASPASO_ACTIVO_FIJO = Producto.TRASPASO_ACTIVO_FIJO;
                productoObj.FECHA_RES_ALTA = Producto.FECHA_RES_ALTA;
                productoObj.NUM_RES_ALTA = Producto.NUM_RES_ALTA;
                productoObj.CON_DOC_DIGITAL_ALTA = Producto.CON_DOC_DIGITAL_ALTA;
                productoObj.RUT_ADMIN_ALTA = Producto.RUT_ADMIN_ALTA;
                productoObj.FECHA_ADMIN_ALTA = Producto.FECHA_ADMIN_ALTA;
                productoObj.ALTO = Producto.ALTO;
                productoObj.ANCHO = Producto.ANCHO;
                productoObj.FONDO = Producto.FONDO; 
                
                productoObj.MATERIALIDAD = Producto.MATERIALIDAD;
                productoObj.COLOR = Producto.COLOR;
                productoObj.PRECIO_UNITARIO_NETO_FACT = Producto.PRECIO_UNITARIO_NETO_FACT;
                productoObj.PRECIO_UNITARIO_BRUTO_FACT = Producto.PRECIO_UNITARIO_BRUTO_FACT;

                productoObj.TOTAL_BRUTO_FACT = Producto.TOTAL_BRUTO_FACT;
                productoObj.TOTAL_LINEA_FACT = Producto.TOTAL_LINEA_FACT;
                productoObj.TOTAL_BRUTO_CALC = Producto.TOTAL_BRUTO_CALC;

                 //db.Entry(productoObj).Property("CANTIDAD").CurrentValue = Producto.CANTIDAD;
                 //db.Entry(productoObj).Property("CANTIDAD_RESTANTE").CurrentValue = Producto.CANTIDAD_RESTANTE;
                 
                productoObj.CANTIDAD = Producto.CANTIDAD;
                productoObj.CANTIDAD_RESTANTE = Producto.CANTIDAD_RESTANTE;

                //actualizar stock
                productoObj.PRODUCTO.STOCK_ACTUAL = productoObj.PRODUCTO.STOCK_ACTUAL + resto;

               
                db.SaveChanges();
                return 1;
            }
            catch(Exception){

                return 0;
            }
        }

        /**
       * Obtener historial de nombres de un proveedor      
       */
        public ActionResult getRazonSocialList(string rutProveedor)
        {
            //string rutProveedor = id;

            if (rutProveedor == null)
            {
                //enviar una lista vacía si el rut del proveedor es nulo
                return View("Partial/_DatosProveedor", new List<RAZON_SOCIAL>());
            }

            try
            {
                //quitar el ultimo numero del rut ya que viene completo
                var rut = rutProveedor.Substring(0, rutProveedor.Length - 1);
                int rutEntero = Convert.ToInt32(rut);

                //enviar a la vista el listado de razones sociales  
                return View("Partial/_DatosProveedor", db.RAZON_SOCIAL.Where(rs => rs.RUT_PROVEEDOR == rutEntero ).OrderByDescending(rs=>rs.FECHA).ToList());
            }
            catch (Exception e)
            {
                //enviar una lista vacía si existe alguna excepcion
                return View("Partial/_DatosProveedor", new List<RAZON_SOCIAL>());
            }

        }

        /* *
         * Buscar datos del proveedor
         * optimizar esta funcion
         * @param: @rut string , desde campo de texto
         */
        [HttpPost]
         public ActionResult getProveedorData(string rut){ 

             RutHelper rutHelper = new RutHelper();

             int _rut = rutHelper.getRut(rut);
             string _dv = rutHelper.getDv(rut);

             //buscar rut en la base de datos, si no existe buscar en web service e insertarlo en la BD 
             PROVEEDOR clienteBD = db.PROVEEDOR.Include(rs=>rs.RAZON_SOCIAL).Where(p => p.RUT == _rut ).FirstOrDefault(); 

            /* *
             * Si existe el rut en la tabla proveedor, 
             *  comparar la ultima razon social con la que existe en el WebService
             *  si son distintas agregarlo a la tabla razon social  
             *  si son iguales hacer nada y retornar el nombre 
             * Si no existe el rut
             *  insertar el nombre en la tabla RAZON_SOCIAL y retornar nombre
             * */
             ClienteWS clienteWS = new ClienteWS(rut);
             string razonSocialLink = "";
             if (clienteBD != null ){
                 
                 var razonSocial = db.RAZON_SOCIAL.Where(rs=>rs.RUT_PROVEEDOR == _rut ).OrderByDescending(rs=>rs.FECHA).FirstOrDefault();

                 string nombreWS = clienteWS.RazonSocial;

                 if (_rut < 60000000)//si es un rut de persona natural.
                 {
                     nombreWS = clienteWS.Nombre + " " + clienteWS.ApePa + " " + clienteWS.ApeMa;
                 }

                 if ((razonSocial != null) && (razonSocial.NOMBRE != nombreWS))
                 { 
                    //insertar en la tabla razon social
                     RAZON_SOCIAL razonSocialObj = new RAZON_SOCIAL();
                     razonSocialObj.RUT_PROVEEDOR = _rut;
                    
                     razonSocialObj.NOMBRE = nombreWS;

                     razonSocialObj.FECHA = System.DateTime.Now;
                     //guardar en tabla RAZON_SOCIAL
                     db.RAZON_SOCIAL.Add(razonSocialObj);
                     db.SaveChanges();
                 } 

                 //retorna el nombre desde BD
                 razonSocialLink = "<a id='razonSocialLink' href='javascript:void(0);'>" + nombreWS + "</a>";
                 return Content(razonSocialLink);
             }
             else {//si no existe en BD

                // ClienteWS clienteWS = new ClienteWS(rut);
                 PROVEEDOR proveedorObj = new PROVEEDOR();
                 RAZON_SOCIAL razonSocialObj = new RAZON_SOCIAL();
                 //insertar en BD el proveedor y su razon social
                 
                 /*PROVEEDOR*/
                 proveedorObj.RUT = _rut;
                 proveedorObj.DV = _dv;
               
                 /*RAZON_SOCIAL*/
                 razonSocialObj.RUT_PROVEEDOR = _rut;
                 if (_rut < 60000000)//si es rut de persona natural
                 {
                     razonSocialObj.NOMBRE = clienteWS.Nombre + " " + clienteWS.ApePa + " " + clienteWS.ApeMa; 
                 }
                 else
                 {
                     razonSocialObj.NOMBRE = clienteWS.RazonSocial;
                     
                 }
                 //if (razonSocialObj != null) {
                 //    razonSocialObj.NOMBRE = clienteWS.RazonSocial;
                 //}
                 //else { razonSocialObj.NOMBRE = clienteWS.Nombre + " " + clienteWS.ApePa + " " + clienteWS.ApeMa; }
                 
                 razonSocialObj.FECHA = System.DateTime.Now; 

                 db.PROVEEDOR.Add(proveedorObj);
                 db.RAZON_SOCIAL.Add(razonSocialObj);
                 db.SaveChanges(); 
                 //enviar una etiqueta <a>
                 razonSocialLink = "<a id='razonSocialLink' href='javascript:void(0);'>" + razonSocialObj.NOMBRE + "</a>";
                 return Content(razonSocialLink);
             }
        }

        /* *
         * Agregar productos al listado
         * 
         */
        [HttpPost]
        public ActionResult NuevoProductoItem(int id, int fila, int cantidad, double total, double? UTM)
        {
            //crear objeto y enviar stock del producto en cuestion
            var producto = db.PRODUCTO.Where(p => p.ID_PRODUCTO == id).FirstOrDefault();
            //double UTM;
            //double total = cantidad * total;

            double precio_unitario = total / cantidad;
            double precio_unitario_final = Math.Round(precio_unitario, 2);
            double precio_bruto = precio_unitario * 1.19;
            int id_tipo_inventario = 1;
            //bool administrable = false;
            //bool inventariable = false;

            if (UTM == null)
            {
                UTM = getUTM();
            }
            
            //si el precio bruto es igual o mayor a la utm, marcar el producto como para activo fijo
            if (precio_bruto >= (3 * UTM))
            {
                // inventariable = true;
                id_tipo_inventario = 2;//activo fijo
            }

            //ADMINISTRABLE = administrable, INVENTARIABLE = inventariable,
            var Factura_producto = new FACTURA_PRODUCTO { 
                                        ID_PRODUCTO = id, 
                                        PRECIO_UNITARIO = (decimal)precio_unitario_final, 
                                        TOTAL_LINEA = (decimal)total, 
                                        CANTIDAD = cantidad, 
                                        ID_TIPO_INV = id_tipo_inventario, 
                                        PRODUCTO = producto, 
                                        VIDA_UTIL = producto.VIDA_UTIL,
                                        TOTAL_LINEA_FACT = (decimal) total, //corresponde al total de la linea sin descto
                                        PRECIO_UNITARIO_NETO_FACT = (decimal)precio_unitario, //corresponde al precio neto sin calculo
                                        PRECIO_UNITARIO_BRUTO_FACT = (decimal)precio_bruto,
                                        TOTAL_BRUTO_FACT = (decimal)(total*1.19) //corresponde al total bruto sin descuento

            };

            //variable para diferenciar las lineas de productos
            ViewBag.FILA = fila;
            //valor total de la linea de productos
            ViewBag.TOTAL = "";
            //retornar a la vista el objeto creado
            ViewBag.TIPO_INVENTARIO = new SelectList(db.TIPO_INVENTARIO.ToList(), "ID_TIPO_INV", "NOMBRE");
            return View("Partial/_NuevoProductoItem", Factura_producto);
        }

        public ActionResult NuevoProductoItemEdit(int id, int fila, int cantidad, double total, double? UTM)
        {
            //crear objeto y enviar stock del producto en cuestion
            var producto = db.PRODUCTO.Where(p => p.ID_PRODUCTO == id).FirstOrDefault();
            //double UTM;
            //double total = cantidad * total;

            double precio_unitario = total / cantidad;
            double precio_unitario_final = Math.Round(precio_unitario, 2);
            double precio_bruto = precio_unitario * 1.19;
            int id_tipo_inventario = 1;
            //bool administrable = false;
            //bool inventariable = false;

            if (UTM == null)
            {
                UTM = getUTM();
            }

            //si el precio bruto es igual o mayor a la utm, marcar el producto como para activo fijo
            if (precio_bruto >= (3 * UTM))
            {
                // inventariable = true;
                id_tipo_inventario = 2;//activo fijo
            }

            //ADMINISTRABLE = administrable, INVENTARIABLE = inventariable,
            var Factura_producto = new FACTURA_PRODUCTO
            {
                ID_PRODUCTO = id,
                PRECIO_UNITARIO = (decimal)precio_unitario_final,
                TOTAL_LINEA = (decimal)total,
                CANTIDAD = cantidad,
                ID_TIPO_INV = id_tipo_inventario,
                PRODUCTO = producto,
                VIDA_UTIL = producto.VIDA_UTIL
            };

            //variable para diferenciar las lineas de productos
            ViewBag.FILA = fila;
            //valor total de la linea de productos
            ViewBag.TOTAL = "";
            //retornar a la vista el objeto creado
            ViewBag.TIPO_INVENTARIO = new SelectList(db.TIPO_INVENTARIO.ToList(), "ID_TIPO_INV", "NOMBRE");
            return View("Partial/_NuevoProductoItem", Factura_producto);
        }

        /* Buscar producto mediante el codigo de barras
         * @id: codigo de barras
         */
        [HttpGet]
        public ActionResult BuscarProductoByCodigoBarra(string id, int fila)
        {
            string codigoBarra = id;
            //var Producto = db.PRODUCTO.Include(p => p.CODIGO_BARRA).Where(p => p.CODIGO_BARRA. == codigoBarra).First();
            //objeto codigo_barra para obtener los datos de producto
            try
            {
                var CodigoDeBarra = db.CODIGO_BARRA.Where(cb => cb.CODIGO == codigoBarra).First();

                //var ProductoObj = new PRODUCTO { ID_PRODUCTO = CodigoDeBarra.ID_PRODUCTO, NOMBRE = CodigoDeBarra.NOMBRE, STOCK_MINIMO = Producto.STOCK_MINIMO, STOCK_ACTUAL = Producto.STOCK_ACTUAL };
                var Factura_producto = new FACTURA_PRODUCTO { 
                    ID_PRODUCTO = CodigoDeBarra.ID_PRODUCTO, 
                    PRECIO_UNITARIO = 0, 
                    TOTAL_LINEA = 0,  
                    CANTIDAD = 1,
                    PRODUCTO = CodigoDeBarra.PRODUCTO, 
                    VIDA_UTIL = CodigoDeBarra.PRODUCTO.VIDA_UTIL
                };
                //variable para diferenciar las lineas de productos
                ViewBag.FILA = fila;
                ViewBag.TIPO_INVENTARIO = new SelectList(db.TIPO_INVENTARIO.ToList(), "ID_TIPO_INV", "NOMBRE");

                return View("Partial/_NuevoProductoItem", Factura_producto);
            }
            catch (Exception e)
            {
                return Content("0");
            }
             
        }
                
        // GET: Factura/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.CLIENTES = new SelectList(db.SP_USUARIOS_BODEGA().ToList(), "RUT", "NOMBRE_COMPLETO");


            if (id == null)
            {
                //Volver a index con mensaje de error
                DisplayErrorMessage("Ingreso no encontrado.");
                return RedirectToAction("Index");
            }

            FACTURA fACTURA = db.FACTURA.Find(id); 

            if (fACTURA == null)
            { 
                DisplayErrorMessage("Ingreso no encontrado.");
                return RedirectToAction("Index");
            }

            //si el proveedor no esta seteado, salir e enviar error.
            if (fACTURA.RUT_PROVEEDOR == null)
            {
                DisplayErrorMessage("El proveedor del ingreso "+id+", no está asignado. Comuníquese con el administrador del sistema.");
                return RedirectToAction("Index");
            }

            //obtener nombre del proveedor
            var proveedor = db.RAZON_SOCIAL.Where(rs => rs.RUT_PROVEEDOR == fACTURA.RUT_PROVEEDOR).OrderByDescending(rs => rs.FECHA).FirstOrDefault();
            //obtener el nombre del servidor 
            string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

            //enviar datos de la factura,orden de compra y proveedor
            ViewBag.CODIGO_OC = fACTURA.CODIGO_OC;
            ViewBag.FECHA_OC = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA_OC);

            if (fACTURA.ARCHIVO_OC != null)
            {
                ViewBag.ARCHIVO_OC = Domain + fACTURA.ARCHIVO_OC;
            }
            else
            {
                ViewBag.ARCHIVO_OC = fACTURA.ARCHIVO_OC;
            }      
           
    
            ViewBag.NUMERO= fACTURA.NUMERO;
            ViewBag.FECHA = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA);
            ViewBag.TOTAL = String.Format("{0:N2}", fACTURA.TOTAL).Replace(",00", "");

            if (fACTURA.ARCHIVO != null)
            {
                ViewBag.ARCHIVO = Domain + fACTURA.ARCHIVO;
            }
            else
            {
                ViewBag.ARCHIVO = fACTURA.ARCHIVO;
            }            

            ViewBag.NETO = String.Format("{0:N2}", fACTURA.NETO).Replace(",00","");
            //String.Format("{0:N2}", fACTURA.NETO);

            ViewBag.DCTO = String.Format("{0:N2}", fACTURA.DESCUENTO).Replace(",00", "");

            ViewBag.SUBTOTAL = String.Format("{0:N2}", fACTURA.SUBTOTAL).Replace(",00", "");
            ViewBag.IVA = String.Format("{0:N2}", fACTURA.IVA).Replace(",00", ""); 


            ViewBag.PROVEEDOR = proveedor.NOMBRE;
            ViewBag.RUT = proveedor.PROVEEDOR.RUT;
            ViewBag.DV = proveedor.PROVEEDOR.DV; 

            ViewBag.LINEA_INVERSION = fACTURA.LINEA_INVERSION.DESC_COMPLETA;

            ViewBag.ID_INGRESO = fACTURA.ID_FACTURA;

            ViewBag.RUT_ADMIN = fACTURA.RUT_ADMIN;

            ViewBag.CORRELATIVO = fACTURA.CORRELATIVO;

            ViewBag.FECHA_INGRESO = String.Format("{0:dd/MM/yyyy}", fACTURA.FECHA_INGRESO);

            //obtener los productos asociados a la factura
            
            var DetalleFactura = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

            if (DetalleFactura.Count == 0) { 
                // Si no hay productos enviar objeto vacio
                var DetalleFacturaVacia = new List<FACTURA_PRODUCTO>();
                return View(DetalleFacturaVacia);
            }

            return View(DetalleFactura);
        }

        //metodo para imprimir el detalle del ingreso en PDF
        public ActionResult DetailsPDF(int? id)
        {
 
            id = 292; 
            var DetalleFactura = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();
 
            var pdf = new PdfResult(DetalleFactura, "DetailsPDF"); 
            pdf.ViewBag.CODIGO_OC = "test";
            return pdf;

        }

        // GET: Factura/Traslado
        //Genera un ingreso de un traslado o de articulos de confeccion propia
        public ActionResult Traslado()
        {
            //Setear fecha de hoy
            ViewBag.FECHA = System.DateTime.Now.ToString("dd-MM-yyyy");

            ViewBag.ORIGEN = new SelectList(db.ORIGEN.Where(o => o.ID_ORIGEN != 1).ToList(), "ID_ORIGEN", "NOMBRE");//enviar traslado y confeccion propia
            /* obtener el listado de productos */
            var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA).ToList();
            //crear una lista de productos para agregar los nombres correspondientes
            List<PRODUCTO> listaProductos = new List<PRODUCTO>();

            foreach (var producto in productos)
            {
                if (producto.SUB_GRUPO.GRUPO.RUBRO.ID_LETRA != 7)
                {
                    producto.NOMBRE = producto.NOMBRE_COMPLETO;
                    //agregar a la lista
                    listaProductos.Add(producto);
                }
                else
                {
                    listaProductos.Add(producto);
                }
            }
            ViewBag.productos = new SelectList(listaProductos, "ID_PRODUCTO", "NOMBRE");
            /* enviar la lista de productos a un elemento DropDown */
            //ViewBag.productos = new SelectList(productos.ToList(), "ID_PRODUCTO", "NOMBRE");
            return View();
        }

        [HttpPost]
        public ActionResult Traslado([Bind(Include = "ID_ORIGEN, NUMERO, TOTAL, FECHA, INSTITUCION")] FACTURA TRASLADO, List<FACTURA_PRODUCTO> FACTURA_PRODUCTO, List<CODIGO_BARRA> CODIGO_BARRA )
        {
            if (ModelState.IsValid || FACTURA_PRODUCTO != null)
            {
                try
                {
                    //registrar administrador que lo ingresa
                    RutHelper cliente = new RutHelper();
                    int rut_admin = cliente.getRut(User.Identity.Name);
                    TRASLADO.RUT_ADMIN = rut_admin;
                    TRASLADO.ID_TIPO_DOC = 2;//resolucion
                    TRASLADO.FECHA_INGRESO = System.DateTime.Now;
                    TRASLADO.ID_LINEA_INVERSION = 1; //linea de inversion predeterminada

                    // Obtener el ultimo correlativo
                    int correlativo = (int)db.FACTURA.Where(f => f.ID_ORIGEN != 1).Max(f => f.CORRELATIVO) + 1;
                    TRASLADO.CORRELATIVO = correlativo;


                    if (TRASLADO.ID_ORIGEN == 2)
                    {//si es traslado asociar Rut Serviu RM

                        TRASLADO.RUT_PROVEEDOR = 61812000;//Serviu RM
                    }
                    else TRASLADO.RUT_PROVEEDOR = 61820004;//Serviu VIII

                    //Compra = 1
                    //Traslado = 2
                    //Confeccion propia = 3
                    //TRASLADO.ID_ORIGEN = 2;

                    // Estado del ingreso
                    // 1 = Activo, 2 = eliminado
                    TRASLADO.ESTADO = 1;

                    db.FACTURA.Add(TRASLADO);

                    //datos para inventario
                    string fecha_res_alta_str = "01-01-1900";
                    DateTime fecha_res_alta =  Convert.ToDateTime(fecha_res_alta_str);

                    string fecha_admin_alta_str = "01-01-1900";
                    DateTime fecha_admin_alta = Convert.ToDateTime(fecha_admin_alta_str);

                    foreach (var item in FACTURA_PRODUCTO)
                    {
                        double precioUnitario = 0;
                        double precioUnitarioIva = 0;
                        double precioUnitarioFinal = 0;
                        double precioUnitarioIvaFinal = 0;
                        int tipoInventario = (int)item.ID_TIPO_INV;
                        
                        precioUnitario = (double)(((double)item.TOTAL_LINEA / 1.19) / item.CANTIDAD);
                        precioUnitarioFinal = Math.Round(precioUnitario, 2);
                        precioUnitarioIva = (double)(item.TOTAL_LINEA/item.CANTIDAD);
                        precioUnitarioIvaFinal = Math.Round(precioUnitarioIva, 2);

                        //ID_ORIGEN = 4 > COMODATO
                        if ((TRASLADO.ID_ORIGEN == 4)  )
                        {
                            tipoInventario = 2;
                        }

                        /* Asignar a cada objeto los parametros correspondientes */
                        FACTURA_PRODUCTO LineaProducto = new FACTURA_PRODUCTO()
                        {
                            ID_FACTURA = TRASLADO.ID_FACTURA,
                            ID_PRODUCTO = item.ID_PRODUCTO,
                            PRECIO_UNITARIO = (decimal)precioUnitarioFinal,
                            PRECIO_UNITARIO_BRUTO = (decimal)precioUnitarioIvaFinal,
                            CANTIDAD = item.CANTIDAD,
                            ID_TIPO_INV = tipoInventario,
                            CANTIDAD_RESTANTE = item.CANTIDAD,
                            TRASPASO_ACTIVO_FIJO = 0,
                            FECHA_RES_ALTA = fecha_res_alta,
                            TOTAL_LINEA = (decimal)precioUnitarioIva*item.CANTIDAD,
                            NUM_RES_ALTA = 0,
                            CON_DOC_DIGITAL_ALTA = 0,
                            RUT_ADMIN_ALTA =  0,
                            FECHA_ADMIN_ALTA = fecha_admin_alta,
                            VIDA_UTIL = 0
                        };

                        db.FACTURA_PRODUCTO.Add(LineaProducto);


                        var producto = db.PRODUCTO.Where(p => p.ID_PRODUCTO == item.ID_PRODUCTO).FirstOrDefault();
                        db.Entry(producto).State = EntityState.Modified;
                        //actualizar stock
                        producto.STOCK_ACTUAL = producto.STOCK_ACTUAL + item.CANTIDAD;

                        db.SaveChanges();

                    }
                   
                    //insertar codigos de barra
                    foreach (var codigoBarra in CODIGO_BARRA)
                    {
                        //buscar si existe el codigo de barras asociado a un producto.
                        //si existe entonces no registrar, en caso contrario, registrarlo.
                        var existe = db.CODIGO_BARRA.Where(cb => cb.CODIGO == codigoBarra.CODIGO && cb.ID_PRODUCTO == codigoBarra.ID_PRODUCTO).Count();
                        if (existe == 0)
                        {//registrar 

                            db.CODIGO_BARRA.Add(codigoBarra);
                            db.SaveChanges();
                        }

                    }
                    //db.SaveChanges();
                    
                    DisplaySuccessMessage("Se ha guardado el traslado.");
                    return RedirectToAction("Traslados");
                }
                catch (Exception e)
                {
                    //DisplayErrorMessage("");
                    //DisplayErrorMessage("No se pudo guardar el traslado, intente nuevamente.");
                    //return RedirectToAction("Traslados");

                    ViewBag.ORIGEN = new SelectList(db.ORIGEN.Where(o => o.ID_ORIGEN != 1).ToList(), "ID_ORIGEN", "NOMBRE");//enviar traslado y confeccion propia

                    ViewBag.RUT_PROVEEDOR = "61820004";//Serviu VIII: 61.820.004-3 <-> Serviu RM: 61.812.000-7
                    //Setear fecha de hoy
                    ViewBag.FECHA = System.DateTime.Now.ToString("dd-MM-yyyy");

                    /* obtener el listado de productos */
                    var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA);

                    /* enviar la lista de productos a un elemento DropDown */
                    ViewBag.productos = new SelectList(productos.ToList(), "ID_PRODUCTO", "NOMBRE");

                    DisplayErrorMessage("No se pudo guardar el traslado, intente nuevamente.");
                    return View();

                }

            }

            ViewBag.ORIGEN = new SelectList(db.ORIGEN.Where(o => o.ID_ORIGEN != 1).ToList(), "ID_ORIGEN", "NOMBRE");//enviar traslado y confeccion propia

            ViewBag.RUT_PROVEEDOR = "61820004";//Serviu VIII: 61.820.004-3 <-> Serviu RM: 61.812.000-7
            //Setear fecha de hoy
            ViewBag.FECHA = System.DateTime.Now.ToString("dd-MM-yyyy");

            /* obtener el listado de productos */
            var productos1 = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA);

            /* enviar la lista de productos a un elemento DropDown */
            ViewBag.productos = new SelectList(productos1.ToList(), "ID_PRODUCTO", "NOMBRE");

            DisplayErrorMessage("No se pudo guardar el traslado, intente nuevamente.");
            return View();
        
        }

        public ActionResult Traslados()
        {
            //listar todos los traslados
            return View(db.FACTURA.Where(f => f.ID_ORIGEN != 1).ToList());
        }
        
        //muestra el detalle de traslado y confeccion propia
        public ActionResult DetalleIngreso(int id)
        {
            //var ingreso = db.FACTURA.Where(f => f.ID_ORIGEN != 1).ToList();

            var Ingreso = db.FACTURA.Find(id);

            //ViewBag.NUMERO_RES = ingreso.NUMERO;
            //ViewBag.FECHA = ingreso.FECHA;
            //ViewBag.INSTITUCION = ingreso.INSTITUCION;
            //ViewBag.ORIGEN = ingreso.ORIGEN.NOMBRE;
            //ViewBag.RUT_ADMIN = ingreso.RUT_ADMIN;
            //ViewBag.TOTAL = ingreso.TOTAL;

            //obtener los productos asociados AL INGRESO
            string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

            ViewBag.PRODUCTOS_INGRESO = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

            if (Ingreso.ARCHIVO != null)
            {
                ViewBag.ARCHIVO = Domain + Ingreso.ARCHIVO;
            }
            else
            {
                ViewBag.ARCHIVO = Ingreso.ARCHIVO;
            }            

            return View(Ingreso);

        }

        public ActionResult ExportData()
        {
            GridView gv = new GridView(); 

            var query =
            from i in db.FACTURA  
            select new
            {
                i.ID_FACTURA, i.LINEA_INVERSION.DESC_LINEA_INVERSION, i.ORIGEN, i.NUMERO, i.FECHA, i.CODIGO_OC, i.FECHA_OC, i.TOTAL
            };

            gv.DataSource = query.ToList();//db.FACTURA.ToList();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Ingresos.xls");
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return RedirectToAction("Index");
        }
        // GET: FACTURAs/FACTURACreate
        //[ActionName("Ingreso")]
        public ActionResult Create()
        {
            ViewBag.ID_LINEA_INVERSION = new SelectList(db.LINEA_INVERSION.Where(li=>li.MOSTRAR_LINEA == true), "ID_LINEA_INVERSION", "DESC_COMPLETA");
            //ViewBag.ID_ORIGEN = new SelectList(db.ORIGEN, "ID_ORIGEN", "NOMBRE");
            //ViewBag.RUT_PROVEEDOR = new SelectList(db.PROVEEDOR, "RUT", "DV");

            ViewBag.RUT_PROVEEDOR = "";
            //ViewBag.UTM = "";// getUTM();
            ViewBag.UTM = getUTM();
            //Setear fecha de hoy
            ViewBag.FECHA = System.DateTime.Now.ToString("dd-MM-yyyy");//{0:dd/MM/yyyy}
            ViewBag.FECHA_OC = System.DateTime.Now.ToString("dd-MM-yyyy");
            /* obtener el listado de productos*/
            var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA).Where(p=>p.MOSTRAR != false).ToList();


            //crear una lista de productos para agregar los nombres correspondientes
            List<PRODUCTO> listaProductos = new List<PRODUCTO>();

            foreach (var producto in productos)
            {
                if (producto.SUB_GRUPO.GRUPO.RUBRO.ID_LETRA != 7)
                {
                    producto.NOMBRE = producto.NOMBRE_COMPLETO;
                    //agregar a la lista
                    listaProductos.Add(producto);
                }
                else
                {
                    listaProductos.Add(producto);
                }
            }


            /* Enviar la lista de productos a un elemento DropDown*/
            ViewBag.productos = new SelectList(listaProductos, "ID_PRODUCTO", "NOMBRE");
            return View();
        }

        // POST: FACTURAs/FACTURACreate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_LINEA_INVERSION,ID_ORIGEN, NUMERO,FECHA, IVA, NETO, DESCUENTO,SUBTOTAL,TOTAL, CODIGO_OC, FECHA_OC, VIDA_UTIL")] FACTURA fACTURA, List<FACTURA_PRODUCTO> FACTURA_PRODUCTO, List<CODIGO_BARRA> CODIGO_BARRA, IEnumerable<HttpPostedFileBase> files)
        {
             
            if (ModelState.IsValid && (FACTURA_PRODUCTO != null))
            {
                try
                {
                    //registrar administrador que lo ingresa
                    RutHelper cliente =  new RutHelper();
                    int rut_admin = cliente.getRut( User.Identity.Name );
                    string rut_proveedor = "";

                    //si el rut es menor a 10 millones
                    if (Request["RUT_PROVEEDOR"].Length < 9)
                    {
                        rut_proveedor = Request["RUT_PROVEEDOR"].Substring(0, 7);
                        fACTURA.RUT_PROVEEDOR = Convert.ToInt32(rut_proveedor);
                    }
                    else if (Request["RUT_PROVEEDOR"].Length >= 9)
                    {
                        rut_proveedor = Request["RUT_PROVEEDOR"].Substring(0, Request["RUT_PROVEEDOR"].Length - 1);
                        fACTURA.RUT_PROVEEDOR = Convert.ToInt32(rut_proveedor);

                    }
                    else if (Request["RUT_PROVEEDOR"].Length == 0)//Si RUT enviado es igual a 0
                    {
                        fACTURA.RUT_PROVEEDOR = 0;
                    }
                    //db.SaveChanges();

                    fACTURA.RUT_ADMIN = rut_admin;
                    //registrar fecha y hora de ingreso
                    fACTURA.FECHA_INGRESO = System.DateTime.Now; 
                
                    fACTURA.ID_ORIGEN = 1;//COMPRA
                    fACTURA.ORIGEN = db.ORIGEN.Find(1);

                    fACTURA.ID_TIPO_DOC = 1; //FACTURA

                    decimal porcentajeDescuento = 0;
                    //si existe % de descuento

                    if (fACTURA.DESCUENTO != 0)
                    {
                        //calcular el % de descuento en base al neto
                        porcentajeDescuento = (decimal)((fACTURA.DESCUENTO * 100) / fACTURA.NETO); 
                    }

                    // Estado del ingreso
                    // 1 = Activo, 0 = eliminado
                    fACTURA.ESTADO = 1;
                    
                    // Obtener el ultimo correlativo
                    int correlativo = (int)db.FACTURA.Where(f => f.ID_ORIGEN == 1).Max(f=>f.CORRELATIVO)+1;

                    fACTURA.CORRELATIVO = correlativo;

                    db.FACTURA.Add(fACTURA);

                    #region insert_archivos
                    //obtener los archivos
                    //Si existe para factura
                    //Request.AcceptTypes.Contains("application/pdf");
                    //se podria generar una funcion parar realizar este procedimiento
                    HttpPostedFileBase ordenCompraFile = Request.Files["ARCHIVO_OC"];
                    HttpPostedFileBase facturaFile = Request.Files["ARCHIVO"];

                    //generar numero aleatorio para el nombre de los archivos
                    Random random = new Random();
                    int aleatorio = random.Next(100, 99999);

                    string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    #region orden_compra
                    //si se subio el archivo
                    if (ordenCompraFile.ContentLength != 0)
                    {
                        //bug: no se puede crear el directorio con la fecha actual
                        string directorioOC = "~/Content/Documentos/OrdenesCompra/";
                       
                        string extensionOC = Path.GetExtension(ordenCompraFile.FileName);
                        //var filename = Path.GetFileName(ordenCompraFile.FileName);

                        string pathOrdenCompra = string.Format("{0}{1}", Server.MapPath(directorioOC), Path.GetFileName(ordenCompraFile.FileName));
                        string nuevoPathOC = string.Format("{0}/{1}", Server.MapPath(directorioOC), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC);
                        string path_OC_BD = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC;


                        //eliminar archivo si existe
                        bool fileExiste = System.IO.File.Exists(nuevoPathOC);

                        if (fileExiste)
                        {
                            nuevoPathOC = string.Format("{0}/{1}", Server.MapPath(directorioOC), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC_" + aleatorio + extensionOC);
                            path_OC_BD = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC_" + aleatorio + extensionOC;

                            //System.IO.File.Delete(pathOrdenCompra);
                        }

                        ordenCompraFile.SaveAs(pathOrdenCompra);
                        //renombrar archivo
                        System.IO.File.Move(pathOrdenCompra, nuevoPathOC);
                        System.IO.File.Delete(pathOrdenCompra);

                        //nuevoPathOC = Domain+"/Bodega/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC;
                        //path para base de datos
                        nuevoPathOC = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC;

                        fACTURA.ARCHIVO_OC = nuevoPathOC;
                    }
                    #endregion

                    /*---Codigo insercion de facturas---*/
                    if (facturaFile.ContentLength != 0)
                    {
                        //bug: no se puede crear el directorio con la fecha actual
                        //string directorioFacturas = "~/Content/Documentos/Facturas/" + System.DateTime.Today.ToString("dd-MM-yyyy");
                        string directorioFacturas = "~/Content/Documentos/Facturas/";
                       
                        string extensionF = Path.GetExtension(facturaFile.FileName);
                        //var filenameFactura = Path.GetFileName(facturaFile.FileName);
                        string pathFacturaFile = string.Format("{0}{1}", Server.MapPath(directorioFacturas), Path.GetFileName(facturaFile.FileName));
                        // string pathFacturaFile = string.Format("{0}/{1}", "~/Content/Documentos/Facturas", Path.GetFileName(facturaFile.FileName));
                        string nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF);
                        string path_FA_BD = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF;
                        
                        
                        bool FactExiste = System.IO.File.Exists(nuevoPathFactura);
                        
                        if (FactExiste)
                        {
                            nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA_" + aleatorio + extensionF);
                            path_FA_BD = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA_" + aleatorio + extensionF;

                        }

                        facturaFile.SaveAs(pathFacturaFile);
                        //renombrar archivo
                        System.IO.File.Move(pathFacturaFile, nuevoPathFactura);
                        System.IO.File.Delete(pathFacturaFile);
                        //path para base de datos
                        nuevoPathFactura = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF;

                        fACTURA.ARCHIVO = nuevoPathFactura;
                    }

                    #endregion


                    //datos para inventario
                    string fecha_res_alta_str = "1900-01-01";
                    DateTime fecha_res_alta = Convert.ToDateTime(fecha_res_alta_str);

                    string fecha_admin_alta_str = "1900-01-01";
                    DateTime fecha_admin_alta = Convert.ToDateTime(fecha_admin_alta_str);

                    /*recorrer productos (LineaProductos) entregados desde la vista*/
                    foreach (var item in FACTURA_PRODUCTO)
                    {
                        double precioUnitario = 0;
                        double descuentoPorLinea = 0;
                        double totalLineaDescuento = 0;
                        double precioUnitarioFinal = 0;

                        double precioUnitarioNetoFact = Math.Round((double)(item.TOTAL_LINEA / item.CANTIDAD), 2, MidpointRounding.ToEven);

                        double total_bruto_fact = (double)(item.TOTAL_LINEA)*1.19;

                        //recalcular los precios unitarios
                        precioUnitario = (double)(item.TOTAL_LINEA / item.CANTIDAD);
                        precioUnitarioFinal = Math.Round(precioUnitario, 2, MidpointRounding.ToEven);

                        if (porcentajeDescuento != 0)
                        {
                            //calcular los precios unitarios con % de descuento

                            descuentoPorLinea = (double)((item.TOTAL_LINEA * porcentajeDescuento) / 100);
                            //restar a cada total de lineas el descuentoPorLinea
                            totalLineaDescuento = ((double)item.TOTAL_LINEA - descuentoPorLinea);
                            //calcular el precio unitario dado el nuevo total de lineas
                            precioUnitario = (double)(totalLineaDescuento / item.CANTIDAD);
                            precioUnitarioFinal = Math.Round(precioUnitario, 2, MidpointRounding.ToEven);

                        }
                        //else
                        //{
                        //    //recalcular los precios unitarios
                        //    precioUnitario = (double)(item.TOTAL_LINEA / item.CANTIDAD);
                        //    precioUnitarioFinal = Math.Round(precioUnitario, 2, MidpointRounding.ToEven);
                        //}

                        //Generar el valor unitario con iva
                        //double precioUnitarioIva = (double)(precioUnitario * 1.19);
                        double precioUnitarioIva = (double)(precioUnitarioFinal * 1.19);

                        
                        /* Asignar a cada objeto los parametros correspondientes */
                        FACTURA_PRODUCTO LineaProducto = new FACTURA_PRODUCTO()
                            {
                                ID_FACTURA = fACTURA.ID_FACTURA,
                                ID_PRODUCTO = item.ID_PRODUCTO,
                                PRECIO_UNITARIO = (decimal)precioUnitario,
                                PRECIO_UNITARIO_BRUTO = (decimal)precioUnitarioIva,
                                CANTIDAD = item.CANTIDAD,
                                TOTAL_LINEA = item.TOTAL_LINEA, //linea neta ingresada por teclado
                                ID_TIPO_INV = item.ID_TIPO_INV,
                                CANTIDAD_RESTANTE = item.CANTIDAD,
                                TRASPASO_ACTIVO_FIJO = 0,
                                FECHA_RES_ALTA = fecha_res_alta,
                                NUM_RES_ALTA = 0,
                                CON_DOC_DIGITAL_ALTA = 0,
                                RUT_ADMIN_ALTA = 0,
                                FECHA_ADMIN_ALTA = fecha_admin_alta,
                                VIDA_UTIL = item.VIDA_UTIL,

                                PRECIO_UNITARIO_NETO_FACT = (decimal)precioUnitarioNetoFact,
                                PRECIO_UNITARIO_BRUTO_FACT = (decimal)precioUnitarioIva,
                                TOTAL_LINEA_FACT = item.TOTAL_LINEA,
                                TOTAL_BRUTO_FACT = (decimal)total_bruto_fact,
                                TOTAL_BRUTO_CALC = (decimal)(precioUnitarioIva * item.CANTIDAD),
                                LINEA_ACTIVA = 1
                               
                               
                            };

                        db.FACTURA_PRODUCTO.Add(LineaProducto);

                        //obtener el id del producto para ingresar el codigo de barra

                        var producto = db.PRODUCTO.Where(p => p.ID_PRODUCTO == item.ID_PRODUCTO).FirstOrDefault();

                        db.Entry(producto).State = EntityState.Modified;

                        //actualizar stock
                        producto.STOCK_ACTUAL = producto.STOCK_ACTUAL + item.CANTIDAD;

                        // buscar si existe el codigo de barras

                        // if(db.CODIGO_BARRA.Where(cb=>cb.CODIGO == Request[""].ToString() ))

                        // crear nuevo codigo barra

                        db.SaveChanges();
                    }

                    //insertar codigos de barra
                    foreach (var codigoBarra in CODIGO_BARRA)
                    {
                        //buscar si existe el codigo de barras asociado a un producto.
                        //si existe entonces no registrar, en caso contrario, registrarlo.
                        var existe =  db.CODIGO_BARRA.Where(cb=>cb.CODIGO == codigoBarra.CODIGO && cb.ID_PRODUCTO == codigoBarra.ID_PRODUCTO).Count();
                        if (existe == 0) {//registrar 

                            db.CODIGO_BARRA.Add(codigoBarra);
                            db.SaveChanges();
                        }

                    }

                    
                //} 
                    //DisplaySuccessMessage("Ingreso realizado con éxito");
                    //return RedirectToAction("Index");
                    TempData["SOLICITUD"] = 0;
                    return RedirectToAction("Details", new { id = fACTURA.ID_FACTURA });

                }
                catch(Exception e){

                    ViewBag.ID_LINEA_INVERSION = new SelectList(db.LINEA_INVERSION.Where(li => li.ACNO == 2015), "ID_LINEA_INVERSION", "DESC_LINEA_INVERSION", fACTURA.ID_LINEA_INVERSION);
                    ViewBag.ID_ORIGEN = new SelectList(db.ORIGEN, "ID_ORIGEN", "NOMBRE", fACTURA.ID_ORIGEN);
                    ViewBag.UTM = getUTM();// getUTM(); 
                    ViewBag.FECHA = System.DateTime.Now.ToString("dd-MM-yyyy"); //fACTURA.FECHA;
                    ViewBag.FECHA_OC = System.DateTime.Now.ToString("dd-MM-yyyy"); //fACTURA.FECHA_OC;
                    ViewBag.RUT_PROVEEDOR = fACTURA.RUT_PROVEEDOR;

                    // ViewBag.RUT_PROVEEDOR = new SelectList(db.PROVEEDOR, "RUT", "DV", fACTURA.RUT_PROVEEDOR);


                    /* obtener el listado de productos*/
                    var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA);
                    //crear una lista de productos para agregar los nombres correspondientes
                    List<PRODUCTO> listaProductos = new List<PRODUCTO>();

                    foreach (var producto in productos)
                    {
                        if (producto.SUB_GRUPO.GRUPO.RUBRO.ID_LETRA != 7)
                        {
                            producto.NOMBRE = producto.NOMBRE_COMPLETO;
                            //agregar a la lista
                            listaProductos.Add(producto);
                        }
                        else
                        {
                            listaProductos.Add(producto);
                        }
                    }
                    
                    /* enviar la lista de productos a un elemento DropDown*/
                    ViewBag.productos = new SelectList(listaProductos, "ID_PRODUCTO", "NOMBRE");

                    DisplayErrorMessage(e.Message);
                    return View(fACTURA);

                } 
            }

            ViewBag.ID_LINEA_INVERSION = new SelectList(db.LINEA_INVERSION.Where(li => li.ACNO == 2015), "ID_LINEA_INVERSION", "DESC_LINEA_INVERSION", fACTURA.ID_LINEA_INVERSION);
            ViewBag.ID_ORIGEN = new SelectList(db.ORIGEN, "ID_ORIGEN", "NOMBRE", fACTURA.ID_ORIGEN);
            ViewBag.UTM = getUTM();// getUTM(); 
            ViewBag.FECHA =  System.DateTime.Now.ToString("dd-MM-yyyy"); //fACTURA.FECHA;
            ViewBag.FECHA_OC = System.DateTime.Now.ToString("dd-MM-yyyy"); //fACTURA.FECHA_OC;
            ViewBag.RUT_PROVEEDOR = fACTURA.RUT_PROVEEDOR;

           // ViewBag.RUT_PROVEEDOR = new SelectList(db.PROVEEDOR, "RUT", "DV", fACTURA.RUT_PROVEEDOR); 
          
            /* Obtener el listado de productos*/
            var productosL = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA);

            /* enviar la lista de productos a un elemento DropDown*/
            ViewBag.productos = new SelectList(productosL.ToList(), "ID_PRODUCTO", "NOMBRE");

            DisplayErrorMessage("No se pudo guardar el ingreso, intente nuevamente.");
            return View(fACTURA);
        }

        // GET: Factura/Edit/5
        public ActionResult Edit(int? id)
        {      

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            /* obtener el objeto factura mendiante el id*/
            FACTURA Factura = db.FACTURA.Find(id);

            /* obtener el listado de productos*/
            var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA); 
            var origenes = db.ORIGEN.ToList();

            var rutProveedor = "";

          
            /* obtener los datos del proveedor */
            //validar si no tiene proveedor
            //si no tiene proveedor asignar un objeto vacio
            if (Factura.PROVEEDOR == null)
            {
                Factura.PROVEEDOR = new PROVEEDOR { RUT = 0, DV = "0" };
            }

            if (Factura.PROVEEDOR.RUT != 0)
            {
                rutProveedor = Factura.PROVEEDOR.RUT + "-" + Factura.PROVEEDOR.DV;
            }
            else {
                rutProveedor = "0";
            }
            
            var razonSocial = db.RAZON_SOCIAL.Where(rs => rs.RUT_PROVEEDOR == Factura.PROVEEDOR.RUT).OrderByDescending(rs => rs.FECHA).First();

            /* enviar la lista de productos a un elemento DropDown*/
            ViewBag.productos = new SelectList(productos.ToList(), "ID_PRODUCTO", "NOMBRE");
            ViewBag.origenes = new SelectList(origenes, "ID_ORIGEN", "NOMBRE");

            /* enviar datos del proveedor */
            ViewBag.rutProveedor = rutProveedor;
            ViewBag.razonSocial = razonSocial.NOMBRE;
           
            /* enviar lineas de inversion*/
            ViewBag.lineasIversion = new SelectList(db.LINEA_INVERSION.Where(li => li.ACNO == 2015).ToList(), "ID_LINEA_INVERSION", "DESC_COMPLETA");

            /* mostrar linea de productos*/
            ViewBag.lineaProductos = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

            if (Factura == null)
            {
                return HttpNotFound();
            }

            return View(Factura);
        }

        // POST: FacturaFACTURA/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_LINEA_INVERSION, ID_ORIGEN,ID_FACTURA, NUMERO, FECHA, IVA, NETO, DESCUENTO,TOTAL, CODIGO_OC, FECHA_OC, ID_TIPO_DOC, RUT_ADMIN, SUBTOTAL, ARCHIVO, ARCHIVO_OC, FECHA_INGRESO, ESTADO, CORRELATIVO")] FACTURA fACTURA, List<FACTURA_PRODUCTO> FACTURA_PRODUCTO, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fACTURA).State = EntityState.Modified;
                string rut_proveedor = "";
 
                //si el rut es menor a 10 millones
                if (Request["RUT_PROVEEDOR"].Length < 9)
                {
                    rut_proveedor = Request["RUT_PROVEEDOR"].Substring(0, 7);
                    fACTURA.RUT_PROVEEDOR = Convert.ToInt32(rut_proveedor);
                }
                else if (Request["RUT_PROVEEDOR"].Length == 9)
                {
                    rut_proveedor = Request["RUT_PROVEEDOR"].Substring(0, Request["RUT_PROVEEDOR"].Length - 1);
                    fACTURA.RUT_PROVEEDOR = Convert.ToInt32(rut_proveedor);

                }
                else if (Request["RUT_PROVEEDOR"].Length == 0)//Si RUT enviado es igual a 0
                {
                    fACTURA.RUT_PROVEEDOR = 0;
                }
                else if ((Request["RUT_PROVEEDOR"].Length > 9))
                {
                    rut_proveedor = Request["RUT_PROVEEDOR"].Substring(0, Request["RUT_PROVEEDOR"].Length - 2);
                    fACTURA.RUT_PROVEEDOR = Convert.ToInt32(rut_proveedor);
                }
                 
                db.SaveChanges();
                //DisplaySuccessMessage("Se ha guardado el ingreso.");

                return RedirectToAction("Details", new { id = fACTURA.ID_FACTURA });
            } 
            /* obtener el listado de productos*/
            var productos = db.PRODUCTO.Include(p => p.SUB_GRUPO).Include(p => p.UNIDAD_MEDIDA);

            /* enviar la lista de productos a un elemento DropDown*/
            ViewBag.productos = new SelectList(productos.ToList(), "ID_PRODUCTO", "NOMBRE");

            DisplayErrorMessage("No se pudo actualizar el ingreso");
            return View(fACTURA);
        }

        //// GET: Factura/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FACTURA fACTURA = db.FACTURA.Find(id);

            if (fACTURA == null)
            {
                return HttpNotFound();
            }
            return View(fACTURA);
        }

        //// POST: Factura/FACTURADelete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FACTURA fACTURA = db.FACTURA.Find(id);

            if (fACTURA == null)
            {
                return HttpNotFound();
            }

            try
            {
                //eliminar el detalle de la factura
                var detalle = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();
                int stock_actual = 0;

                foreach (var item in detalle)
                {
                    //Buscar el producto para restar el stock
                    var prod = db.PRODUCTO.Find(item.ID_PRODUCTO);
                    stock_actual = (int)(prod.STOCK_ACTUAL - item.CANTIDAD);

                    db.Entry(prod).State = EntityState.Modified;
                    db.Entry(prod).Property("STOCK_ACTUAL").CurrentValue = stock_actual;

                    //dejar los FP con cantidades igual a cero
                    db.Entry(item).State = EntityState.Modified;
                    db.Entry(item).Property("CANTIDAD_RESTANTE").CurrentValue = 0;
                    //db.FACTURA_PRODUCTO.Remove(item);
                }

                //db.FACTURA.Remove(fACTURA);
                db.Entry(fACTURA).State = EntityState.Modified;
                db.Entry(fACTURA).Property("ESTADO").CurrentValue = 0;

                db.SaveChanges();

                DisplaySuccessMessage("Registro Borrado");
                return RedirectToAction("Index");
            }   
            catch (Exception e) {

                DisplayErrorMessage("No se puede eliminar el ingreso porque esta asociado a salidas de materiales.");
                return RedirectToAction("Index");
            } 
        }

        private void DisplaySuccessMessage(string msgText)
        {
            TempData["SuccessMessage"] = msgText;
        }

        private void DisplayErrorMessage(string msgText)
        {
            TempData["ErrorMessage"] = msgText;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region utils
 
        public string Completa(string valor, int largo, string caracter, int orientacion){
            
            int o;
            string ceros;
            ceros = "";
            if ( valor == null)
            {
                valor = "";
            }
            for (o = 1; (o<= (largo - valor.ToString().TrimEnd().TrimStart().Length)); o++)
            {
                ceros = (ceros + caracter);
            }

            if ((orientacion == 1))
            {
                ceros = (ceros + valor);
            }
            else
            {
                ceros = (valor + ceros);
            }
            return ceros.Substring(0, largo);
        }

        public int getUTM()
        {
           string primerDia = DateTime.Now.Year.ToString() + "-" + Completa(DateTime.Now.Month.ToString(), 2, "0", 1) + "-01";
           string ultimoDia = DateTime.Now.Year.ToString() + "-" + Completa(DateTime.Now.Month.ToString(), 2, "0", 1) + "-" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString();

            var indice = new Indicadores_Minvu.IndiceSector();
            var consulta =  new Indicadores_Minvu.IndicesSector_IndiceSectorOrchestration_prt_IndicesSectorSoapClient();

            indice.Indice = 4;
             
            indice.FechaInicio = Convert.ToDateTime(primerDia);
            indice.FechaTermino = Convert.ToDateTime(ultimoDia);

            var c = consulta.ObtenerIndicesSector(indice).Response; 

            return Convert.ToInt32(c[0].Valor);
        }
         
        /*Obtiene el valor de un mes y año especifico*/
        [HttpPost]
        public int getUTMByFecha(string mes, string anio)
        {
            //string primerDia = fecha.Year.ToString() + "-" + Completa(fecha.Month.ToString(), 2, "0", 1) + "-01";
            string primerDia = anio + "-" + mes + "-01";
            //string ultimoDia = fecha.Year.ToString() + "-" + Completa(fecha.Month.ToString(), 2, "0", 1) + "-" + DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();
            string ultimoDia = anio + "-" + mes + "-" + DateTime.DaysInMonth(Convert.ToInt32(anio), Convert.ToInt32(mes)).ToString();
            
            var indice = new Indicadores_Minvu.IndiceSector();
            var consulta = new Indicadores_Minvu.IndicesSector_IndiceSectorOrchestration_prt_IndicesSectorSoapClient();

            indice.Indice = 4;

            indice.FechaInicio = Convert.ToDateTime(primerDia);
            indice.FechaTermino = Convert.ToDateTime(ultimoDia);

            var c = consulta.ObtenerIndicesSector(indice).Response;

            //convertir el decimal en entero y retornar
            return Convert.ToInt32(c[0].Valor);
        }

        #endregion

        [HttpGet]
        public ActionResult getEntregasPorDependencia(int id)
        {
            //obtener todas las salidas
            List<SALIDA_DETALLE> salidas = db.SALIDA_DETALLE.Where(sd => sd.ID_SALIDA == sd.SALIDA.ID_SALIDA && sd.SALIDA.ID_SOLICITUD == sd.SALIDA.SOLICITUD.ID_SOLICITUD && sd.SALIDA.SOLICITUD.ID_DEPENDENCIA == 43).ToList();
            List<int> codigosProductos = new List<int>(); //array o lista? que sera mas eficiente?
            //de cada salida, obtener los productos para luego sumar sus cantidades
            foreach (var item in salidas)
            {
                codigosProductos.Add(item.FACTURA_PRODUCTO.ID_PRODUCTO);
            }

            //aplicar distinct a la lista de ID's de productos
            var codigosProductosDsct = (from n in codigosProductos select n).Distinct();

            //var nose = (from n in codigosProductos select n);

            //por cada id de producto consultar en la salida y obtener la cantidad para sumar
            List<int>[] lista = new List<int>[codigosProductosDsct.Count()];

            List<ProductoCantidadSalida> productoCantidad = new List<ProductoCantidadSalida>();

            foreach (var codigo in codigosProductosDsct)//filas
            {
                int sumaProductos = 0;
                decimal costoTotal = 0;

                foreach (var salida in salidas)
                {
                    if (salida.FACTURA_PRODUCTO.ID_PRODUCTO == codigo)
                    {
                         //calcular el costo total (CANTIDAD ENTREGADA * PRECIO UNITARIO BRUTO)
                        costoTotal = costoTotal + ((decimal)(salida.CANTIDAD * salida.FACTURA_PRODUCTO.PRECIO_UNITARIO_BRUTO));

                        //sumar la cantidad total
                        sumaProductos = sumaProductos + (int)salida.CANTIDAD;

                    }
                }

                productoCantidad.Add(new ProductoCantidadSalida { codigoProducto = codigo, cantidadSalida = sumaProductos, costoTotal = costoTotal });
               
            }

            //recorrer las listas para agregar el nombre del producto
            foreach (var prod in productoCantidad)
            { 
                prod.nombreProducto = db.PRODUCTO.Where(p => p.ID_PRODUCTO == prod.codigoProducto).FirstOrDefault().NOMBRE;
            }

            ComunHelper comun = new ComunHelper();
            ViewBag.DEPENDENCIA = comun.getNombreDependencia(id, db);

            //retornar la lista del tipo "ProductoCantidadSalida"
            return View(productoCantidad);

        }
        
        public ActionResult getLineasDeInversion(int acno)
        {

            var json = new JavaScriptSerializer();
            var lineas = db.LINEA_INVERSION.Where(li=>li.ACNO == acno).ToList();

            var p = (from pp in lineas select new { pid = pp.ID_LINEA_INVERSION, pname = pp.DESC_COMPLETA });

            return Content(json.Serialize(p));

        }

        //file upl
        public ActionResult SaveFileOtros()
        {
            try
            {
                int id_factura = Convert.ToInt32(Request.Form["ID_FACTURA"]); 
                //obtener la factura
                FACTURA fACTURA = db.FACTURA.Find(id_factura); 

                HttpPostedFileBase facturaFile = null;
                facturaFile = Request.Files["ARCHIVO"];

                //generar numero aleatorio para el nombre de los archivos
                Random random = new Random();
                int aleatorio = random.Next(100, 99999);

                string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
 

                /*---Codigo insercion de facturas---*/
                if (facturaFile.ContentLength != 0)
                {
                    //string directorioFacturas = "~/Content/Documentos/Facturas/" + System.DateTime.Today.ToString("dd-MM-yyyy");
                    string directorioFacturas = "~/Content/Documentos/Otros/";

                    string extensionF = Path.GetExtension(facturaFile.FileName);
                    string pathFacturaFile = string.Format("{0}{1}", Server.MapPath(directorioFacturas), Path.GetFileName(facturaFile.FileName));
                    string nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_RES" + extensionF);
                    //path para base de datos
                    string path_RES_BD = "/Bodega/Content/Documentos/Otros/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_RES" + extensionF;

                    //eliminar archivo si existe
                    bool facturaExiste = System.IO.File.Exists(nuevoPathFactura);

                    if (facturaExiste)//si el archivo existe, mantener, pero poner un numero aleatorio al final.
                    {
                        //System.IO.File.Delete(pathOrdenCompra);
                        nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_RES_" + aleatorio + extensionF);
                        path_RES_BD = "/Bodega/Content/Documentos/Otros/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_RES_" + aleatorio + extensionF;
                    }

                    facturaFile.SaveAs(pathFacturaFile);

                    //renombrar archivo
                    System.IO.File.Move(pathFacturaFile, nuevoPathFactura);

                    System.IO.File.Delete(pathFacturaFile);

                    db.Entry(fACTURA).Property("ARCHIVO").CurrentValue = path_RES_BD;
                    db.SaveChanges();
                }
 
                DisplaySuccessMessage("Archivos subidos correctamente.");
                return RedirectToAction("Traslados");//Listado de otros
            }
            catch (Exception e)
            {
                //DisplayErrorMessage("No se pudo realizar la carga de archivos.");
                DisplayErrorMessage(e.Message);
                return RedirectToAction("Traslados");//Listado de otros
            }
        }

        //@id: id factura (Ingreso)
        public ActionResult SaveFile()
        {
            try
            {
                int id_factura = Convert.ToInt32(Request.Form["ID_FACTURA"]);

                //obtener la factura
                FACTURA fACTURA = db.FACTURA.Find(id_factura);

                HttpPostedFileBase ordenCompraFile = null;
                ordenCompraFile =   Request.Files["ARCHIVO_OC"];

                HttpPostedFileBase facturaFile = null;
                facturaFile = Request.Files["ARCHIVO"];

                //generar numero aleatorio para el nombre de los archivos
                Random random = new Random();
                int aleatorio = random.Next(100, 99999);

                string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);


                //si se subio la orden de compra
                if ( ordenCompraFile.ContentLength != 0)
                {
                    //bug: no se puede crear el directorio con la fecha actual
                    string directorioOC = "~/Content/Documentos/OrdenesCompra/";

                    string extensionOC = Path.GetExtension(ordenCompraFile.FileName);

                    string pathOrdenCompra = string.Format("{0}{1}", Server.MapPath(directorioOC), Path.GetFileName(ordenCompraFile.FileName));

                    string nuevoPathOC = string.Format("{0}/{1}", Server.MapPath(directorioOC), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC);
                    string path_OC_BD = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC;


                    //eliminar archivo si existe
                    bool fileExiste = System.IO.File.Exists(nuevoPathOC);

                    if (fileExiste)//si el archivo existe, mantener, pero poner un numero aleatorio al final.
                    {
                        //System.IO.File.Delete(pathOrdenCompra);
                        nuevoPathOC = string.Format("{0}/{1}", Server.MapPath(directorioOC), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC_" + aleatorio + extensionOC);
                        path_OC_BD = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC_" + aleatorio + extensionOC;
                    }
                    ordenCompraFile.SaveAs(pathOrdenCompra);

                    //renombrar archivo
                    System.IO.File.Move(pathOrdenCompra, nuevoPathOC);
                    System.IO.File.Delete(pathOrdenCompra);
                    
                    //path para base de datos
                    //nuevoPathOC = "/Bodega/Content/Documentos/OrdenesCompra/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_OC" + extensionOC;

                    fACTURA.ARCHIVO_OC = nuevoPathOC;

                    db.Entry(fACTURA).Property("ARCHIVO_OC").CurrentValue = path_OC_BD;
                    db.SaveChanges();
                }

                /*---Codigo insercion de facturas---*/
                if ( facturaFile.ContentLength != 0)
                {
                    //string directorioFacturas = "~/Content/Documentos/Facturas/" + System.DateTime.Today.ToString("dd-MM-yyyy");
                    string directorioFacturas = "~/Content/Documentos/Facturas/";

                    string extensionF = Path.GetExtension(facturaFile.FileName);
                    string pathFacturaFile = string.Format("{0}{1}", Server.MapPath(directorioFacturas), Path.GetFileName(facturaFile.FileName));
                    string nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF);
                    //path para base de datos
                    string path_FA_BD = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF;

                    //eliminar archivo si existe
                    bool facturaExiste = System.IO.File.Exists(nuevoPathFactura);

                    if (facturaExiste)//si el archivo existe, mantener, pero poner un numero aleatorio al final.
                    {
                        //System.IO.File.Delete(pathOrdenCompra);
                        nuevoPathFactura = string.Format("{0}/{1}", Server.MapPath(directorioFacturas), fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA_" + aleatorio + extensionF);
                        path_FA_BD = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA_" + aleatorio + extensionF;
                    }

                    facturaFile.SaveAs(pathFacturaFile);

                    //renombrar archivo
                    System.IO.File.Move(pathFacturaFile, nuevoPathFactura); 
                    System.IO.File.Delete(pathFacturaFile);
                    
                    //nuevoPathFactura = "/Bodega/Content/Documentos/Facturas/" + fACTURA.ID_FACTURA + System.DateTime.Today.ToString("ddMMyyyy") + "_FA" + extensionF;
                    //fACTURA.ARCHIVO = nuevoPathFactura;

                    db.Entry(fACTURA).Property("ARCHIVO").CurrentValue = path_FA_BD;
                    db.SaveChanges();
                }

                DisplaySuccessMessage("Archivos subidos correctamente.");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                //DisplayErrorMessage("No se pudo realizar la carga de archivos.");
                DisplayErrorMessage(e.Message);
                return RedirectToAction("Index");
            }

        }

        //@id: id orden de compra
        //Ajax
        public ActionResult ExisteOrdenCompra(string oc)
        {
            //buscar si existe una factura con el mismo orden de compra

            var factura = db.FACTURA.Where(f => f.CODIGO_OC == oc && f.ESTADO != 0).FirstOrDefault();
            
            if (factura != null)
            {
                return Content("1");
            }
            else return Content("0");
        }

        public ActionResult EditLineaProducto(int id)
        {

            ViewBag.INVENTARIO = new SelectList(db.TIPO_INVENTARIO.ToList(), "ID_TIPO_INV", "NOMBRE");

            
            return View("Partial/_EditLineaProducto", db.FACTURA_PRODUCTO.Where(df => df.ID_FP == id).First());
            //return Content(id.ToString());
        }

        [HttpPost]
        public ActionResult EditLineaProducto([Bind(Include = "ID_FP, ID_FACTURA , ID_PRODUCTO ,MARCA ,MODELO ,NUMERO_SERIE ,ESTADO ,UBICACION ,DEPENDENCIA ,CODIGO_INVENTARIO ,ASIGNADO_A ,OBSERVACION ,VIDA_UTIL ,ALTO ,ANCHO ,FONDO ,COLOR ,MATERIALIDAD ,FORMA ,NUMERO_MOTOR ,NUMERO_CHASIS ,PATENTE ,CANTIDAD_RESTANTE, TIPO_EQUIPO ,TRASPASO_ACTIVO_FIJO ,FECHA_RES_ALTA ,NUM_RES_ALTA ,CON_DOC_DIGITAL_ALTA ,RUT_ADMIN_ALTA ,FECHA_ADMIN_ALTA")] FACTURA fACTURA)
        {
            return RedirectToAction("Edit", new { id = fACTURA.ID_FACTURA });
        }

        public ActionResult inicializarCorrelativoCompras()
        {
            //obtener todos los ingresos con ID_origen  = 1
            var compras = db.FACTURA.Where(f => f.ID_ORIGEN == 1).ToList().OrderBy(f=>f.ID_FACTURA);

            int cant_compras = compras.Count();
            int i = 0;

            foreach (var item in compras)
            {
                item.CORRELATIVO = ++i;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            return Content("Exito");

        }

        public ActionResult inicializarCorrelativoOtros()
        {
            //obtener todos los ingresos con ID_origen  = 1
            var compras = db.FACTURA.Where(f => f.ID_ORIGEN == 2).OrderBy(f => f.ID_FACTURA).ToList();

            int cant_compras = compras.Count();
            int i = 0;

            foreach (var item in compras)
            {
                item.CORRELATIVO = ++i;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Content("Exito");

        }

        /*
         * Verificar si la sumatoria de los precios con iva (en FACTURA_PRODUCTO)
           de un ingreso corresponde al total en FACTURA.
         */         
        public ActionResult VerificaIngreso()
        {
            //obtener todos los ingresos
            var ingresos = db.FACTURA.ToList();
            decimal sumaIva = 0;
            decimal diff = 0;
            decimal precioBrutoFinal = 0;
            string mensaje = "";
            //recorrer las facturas obteniendo sus lineas de ingreso 
            foreach (var item in ingresos)
            {

                //decimal sumaIva = 0;
                //obtiene las lineas de ingreso de una factura ordenadas de menor a mayor por precio bruto
                var lineaIngreso = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == item.ID_FACTURA).OrderBy(fp=>fp.PRECIO_UNITARIO_BRUTO).ToList();
                decimal minimoBruto = 0;
                decimal maximoBruto = 0;
                decimal precioNeto = 0;
                decimal iva = 1.19M;
                int dife = 0;
                foreach (var linea in lineaIngreso)
                { 
                    sumaIva = (decimal)(sumaIva+linea.PRECIO_UNITARIO_BRUTO * linea.CANTIDAD);
                }
                //sumar los precios brutos  por cada linea de prod
                //sumaIva = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == item.ID_FACTURA).Sum(fp => fp.TOTAL_LINEA).Value;
                
                //si la suma de los precios brutos es mayor al total de factura, entonces ajustar 
                //los valores unitarios con la cantidad restante.
                if (sumaIva > item.TOTAL)
                {
                    //obtener diferencia
                    diff = sumaIva - (decimal)item.TOTAL;
                    maximoBruto = (decimal)lineaIngreso.Max(fp => fp.PRECIO_UNITARIO_BRUTO);
                    precioBrutoFinal = Math.Round(lineaIngreso.First().PRECIO_UNITARIO_BRUTO.Value - diff, MidpointRounding.AwayFromZero);
                    precioNeto = (decimal)(precioBrutoFinal / (1.19M));

                    db.Entry(lineaIngreso.First()).State = EntityState.Modified;
                    db.Entry(lineaIngreso.First()).Property("PRECIO_UNITARIO_BRUTO").CurrentValue = precioBrutoFinal;
                    db.Entry(lineaIngreso.First()).Property("PRECIO_UNITARIO").CurrentValue = Math.Round(precioNeto, MidpointRounding.AwayFromZero);
                    db.SaveChanges();
                    mensaje = "Se han encontrado diferencias. ingreso numero:" + item.CORRELATIVO + " " + "suma iva =" + sumaIva + " total factura:" + item.TOTAL + "<br>" + mensaje;

                }
                else if (sumaIva < item.TOTAL)
                {
                    //si el total es mayor que la sumatoria de los valores brutos, agregar el delta a uno de los valores brutos.
                    //obtener el menor de los precios brutos
                    minimoBruto = (decimal)lineaIngreso.Min(fp=>fp.PRECIO_UNITARIO_BRUTO);

                    //obtener diferencia
                    diff = (decimal)item.TOTAL - sumaIva;
                    dife = (int)diff;
                    //sumar la diferencia al precio bruto menor
                    precioBrutoFinal = Math.Round(lineaIngreso.First().PRECIO_UNITARIO_BRUTO.Value + diff, MidpointRounding.AwayFromZero);

                    precioNeto = (decimal) (precioBrutoFinal / (1.19M));
                    //actualizar la fila FP correspondiente
                    db.Entry(lineaIngreso.First()).State = EntityState.Modified;
                    db.Entry(lineaIngreso.First()).Property("PRECIO_UNITARIO_BRUTO").CurrentValue = precioBrutoFinal;
                    db.Entry(lineaIngreso.First()).Property("PRECIO_UNITARIO").CurrentValue = Math.Round(precioNeto, MidpointRounding.AwayFromZero);
                    db.SaveChanges();

                    mensaje = "Se han encontrado diferencias. ingreso numero:"+item.CORRELATIVO+" "+"suma iva ="+sumaIva+" total factura:"+ item.TOTAL +"<br>"+mensaje;

                }
                else if(sumaIva < item.TOTAL){
                    mensaje = "no se han encontrado diferencias. ingreso numero:" + item.CORRELATIVO +"<br>" + mensaje;

                }
                //si existen diferencias, alertar
                //if (sumaIva != item.TOTAL)
                //{
                //  return  Content("Se han encontrado diferencias. ingreso numero:"+item.CORRELATIVO+" "+"suma iva ="+sumaIva+" total factura:"+ item.TOTAL+" la diferencia es de ");

                //}

                sumaIva = 0;
            }
            return Content(mensaje);

        }

        public ActionResult AjustarIngreso(int id)
        {
            var ingreso = db.FACTURA.Find(id);
            //lista de netos sin decimales
            List<decimal> lineasNetoSinDec = new List<decimal>();
            //lista de brutos sin decimales
            List<decimal> lineasBrutoSinDec = new List<decimal>();

            //obtener las lineas de productos del ingreso
            var lineas = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();
            decimal totalNeto = 0;
            

            //actualizar todas las lineas de ingreso
            foreach(var liN in lineas)
            {
                totalNeto = Math.Truncate((decimal)(liN.PRECIO_UNITARIO * liN.CANTIDAD));
                lineasNetoSinDec.Add(totalNeto);

                //actualizar cada linea.
                liN.PRECIO_UNITARIO = Math.Truncate((decimal)liN.PRECIO_UNITARIO);
                liN.PRECIO_UNITARIO_BRUTO = Math.Truncate((decimal)liN.PRECIO_UNITARIO_BRUTO);

                db.Entry(liN).State = EntityState.Modified;
                db.SaveChanges();

                totalNeto = 0;
            }

            /*
            foreach (var liB in lineas)
            { 
                totalNeto =  Math.Truncate((decimal)(liB.PRECIO_UNITARIO * liB.CANTIDAD));
                lineasNetoSinDec.Add(totalNeto);
                totalNeto = 0;
            }
            */

           //sumar los precios netos de las lineas
            decimal sumaNetoEntero = lineasNetoSinDec.Sum();

            //sumar precios + iva
            decimal sumaBrutoEntero = lineasBrutoSinDec.Sum();

            decimal neto = (decimal)ingreso.NETO;
            decimal total = (decimal)ingreso.TOTAL;

            //obtener el resto al restar el neto con sumaNetoEntero

            decimal restoNeto = neto - sumaNetoEntero;
            decimal restoTotal = total - sumaBrutoEntero;

            //el resto neto si es positivo debo sumarla en la lina de menos de menor valor
            
            //var menorLinea = lineas.Min(l => l.PRECIO_UNITARIO); 
            var menorLinea = lineas.OrderBy(l=>l.PRECIO_UNITARIO).First();

            //modifico el valor actual mas el resto
            decimal sumaRestoNeto = restoNeto + (decimal)menorLinea.PRECIO_UNITARIO;

            //actualizar la fila correspondiente 
             
            return Content("2");

        }

        public ActionResult AjustarPreciosBrutos(int id)
        {
            var ingreso = db.FACTURA.Find(id);
            decimal resto = 0;
            //obtener las lineas de productos del ingreso
            var lineas = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id);

            decimal auxiliarBruto = 0;
            decimal sumatoriaBrutos = 0;

            //quitar los decimales y actualizar todas las lineas de ingreso brutas
            foreach (var liB in lineas.ToList())
            {
                auxiliarBruto = Math.Truncate((decimal)liB.PRECIO_UNITARIO_BRUTO);
                //sumar todas las lineas brutas
                sumatoriaBrutos +=(auxiliarBruto * (int)liB.CANTIDAD);

                //actualizar cada linea.
                liB.PRECIO_UNITARIO_BRUTO = auxiliarBruto;

                db.Entry(liB).State = EntityState.Modified;
                db.SaveChanges();
            }

            //ahora hacer la diferencia entre el total ingresado y la suma de los valores sin decimales
            resto = (decimal)ingreso.TOTAL - sumatoriaBrutos;

            //si es < 0, sumatoriaBrutos es mayor que el total ingresado
            if (resto > 0)
            {
                //si es mayor a cero, el resto se debe sumar a la linea de mayor/menor valor
                var lineaMenor = lineas.OrderBy(l => l.PRECIO_UNITARIO_BRUTO).First(); 

                //actualizar esta linea agregandole el resto

                //actualizar cada linea.
                lineaMenor.PRECIO_UNITARIO_BRUTO =  lineaMenor.PRECIO_UNITARIO_BRUTO + resto;

                db.Entry(lineaMenor).State = EntityState.Modified;
                db.SaveChanges();

            }
            else if (resto < 0)
            {
                //restar el resto a la linea de mayor valor.
                
            }
            else
            {
                //son iguales y no se hace nada

            }


            return Content("2");
        }

        //Eliminar (anular) ingreso:
        /*
         * Cambia el estado del ingreso a 0, no borra la fila en la tabla FACTURA.
         * Actualiza la tabla FACTURA_PRODUCTO dejando la cantidad restante 0
         * para no tener salidas posteriores de este ingreso anulado.
         */
        public ActionResult Eliminar(int id)
        {
            var ingreso = db.FACTURA.Find(id);

            //si el ingreso tiene salidas, no eliminar
            if (ingreso != null)
            {
                int salidas = db.SALIDA_DETALLE.Where(s => s.FACTURA_PRODUCTO.FACTURA.ID_FACTURA == id).Count();

                if (salidas > 0)
                {
                    DisplayErrorMessage("El ingreso tiene salidas de bodega. No se puede eliminar.");
                   return RedirectToAction("Index"); 
                }

            }
           
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Dejar el ingreso con estado desactivado (0)
                    db.Entry(ingreso).State = EntityState.Modified;
                    db.Entry(ingreso).Property("ESTADO").CurrentValue = 0;

                    //obtener las FP y a cada FP poner cantidad_restante = 0 y linea_activa = 0
                    //traer todas las lineas de ingreso asociadas a la factura y desactivarla
                    var detalle_ingreso = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

                    foreach (var detalle in detalle_ingreso)
                    {
                        db.Entry(detalle).State = EntityState.Modified;
                        db.Entry(detalle).Property("CANTIDAD_RESTANTE").CurrentValue = 0;
                        db.Entry(detalle).Property("LINEA_ACTIVA").CurrentValue = 0;

                        //Actualizar el stock
                        db.Entry(detalle.PRODUCTO).State = EntityState.Modified;
                        
                        db.Entry(detalle.PRODUCTO).Property("STOCK_ACTUAL").CurrentValue = detalle.PRODUCTO.STOCK_ACTUAL - detalle.CANTIDAD;
                    }

                    db.SaveChanges();
                    dbContextTransaction.Commit(); 

                    DisplaySuccessMessage("El ingreso " + ingreso.CORRELATIVO + " ha sido enviado a la papelera.");
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    DisplayErrorMessage("No se ha podido borrar el ingreso");
                    return RedirectToAction("Index");
                }
                
            }

        }
    
        //Recuperar ingreso:
        /*
         * Cambia el estado del ingreso a 1
         * Actualiza la CANTIDAD_RESTANTE en la tabla FACTURA_PRODUCTO 
         * El valor de CANTIDAD_RESTANTE sera el mismo que CANTIDAD 
         * Actualizar el stock
         */
        public ActionResult Recuperar(int id)
        {
            var ingreso = db.FACTURA.Find(id);

            if (ingreso == null)
            {
                DisplayErrorMessage("Ingreso no encontrado");
                RedirectToAction("Papelera");
            }

            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {

                    var detalle_ingreso = db.FACTURA_PRODUCTO.Where(fp => fp.ID_FACTURA == id).ToList();

                    db.Entry(ingreso).State = EntityState.Modified;
                    db.Entry(ingreso).Property("ESTADO").CurrentValue = 1;

                    foreach (var detalle in detalle_ingreso)
                    {
                        db.Entry(detalle).State = EntityState.Modified;
                        db.Entry(detalle).Property("CANTIDAD_RESTANTE").CurrentValue = detalle.CANTIDAD;
                        db.Entry(detalle).Property("LINEA_ACTIVA").CurrentValue = 1;
                        
                        //Actualizar el stock
                        db.Entry(detalle.PRODUCTO).State = EntityState.Modified;
                        db.Entry(detalle.PRODUCTO).Property("STOCK_ACTUAL").CurrentValue = detalle.PRODUCTO.STOCK_ACTUAL + detalle.CANTIDAD; 
                    }

                    db.SaveChanges();
                    dbContextTransaction.Commit(); 
                     
                    DisplaySuccessMessage("El ingreso numero " + ingreso.CORRELATIVO + " fue recuperado con exito.");
                    return RedirectToAction("index");
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(e.Message);
                    return RedirectToAction("Papelera");

                }
            }
           
        }
    
    }//End class

}
