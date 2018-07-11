using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ep24.web.Models;
using ep24.web.Repositories;

namespace ep24.web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class OrderController : Controller
    {
        private IProductRepository productRepo;
        private IOrderRepository orderRepo;

        public OrderController(IProductRepository productRepo, IOrderRepository orderRepo)
        {
            this.productRepo = productRepo;
            this.orderRepo = orderRepo;
        }

        [HttpGet]
        public IEnumerable<Order> ListHistory()
        {
            //TODO: implement scenario ขอรายการสั่งซื้อที่ยืนยันรายการสั่งซื้อแล้ว ep24
            return orderRepo.List(o => o.PaidDate.HasValue);
            //throw new NotImplementedException();
        }

        [HttpPost]
        public OrderProductResponse OrderProduct([FromBody]OrderProductRequest request)
        {
            //เสร็จTODO: implement scenario ไม่มีข้อมูล หรือไม่เลือกสินค้าที่จะสั่ง ให้แจ้งกลับว่า 'ไม่พบเมนูที่จะสั่ง' และไม่บันทึกข้อมูล ep 23
            if(request == null || request.OrderedProducts== null|| !request.OrderedProducts.Any())
            {
                return new OrderProductResponse { Message = "ไม่พบเมนูที่จะสั่ง", };
            }

            var productIds = request.OrderedProducts.Select(p => p.Key); 
            var products = productRepo.GetAllProducts();
            var filteredProducts = products.Where(p => productIds.Contains(p.Id)).ToList();

            if (filteredProducts.Count() != productIds.Count()) 
            {
                return new OrderProductResponse { Message = "ไม่พบสินค้าบางรายการ กรุณาสั่งใหม่อีกครั้ง", };
            }

            if (filteredProducts.Any(p => p.HasStock && p.Stock < request.OrderedProducts.First(op => op.Key == p.Id).Value))
            {
                return new OrderProductResponse { Message = "สินค้าบางรายการมีไม่พอ กรุณาสั่งใหม่อีกครั้ง", };
            }
            var id = Guid.NewGuid().ToString();
            var order = new Order
            {
                Id = id,
                OrderedProducts = filteredProducts,
                ReferenceCode = id.Substring(0, 5),
                OrderDate = DateTime.UtcNow,
                Username = request.Username,
            };
            orderRepo.Create(order);
            
            //เสร็จTODO: implement scenario สั่งเมนูสำเร็จ ให้แจ้งกลับว่า 'สั่งเมนูสำเร็จ กรุณาชำระเงินที่เค้าเตอร์' พร้อมกับ ReferenceCode ep23
            return new OrderProductResponse { Message = "สั่งเมนูสำเร็จ กรุณาชำระเงินที่เค้าเตอร์",ReferenceCode= order.ReferenceCode, };
        }
    }
}
