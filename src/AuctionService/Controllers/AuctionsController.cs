using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController: ControllerBase
{
    private readonly AuctionDbContext context;
    private readonly IMapper mapper;

    public AuctionsController(AuctionDbContext context,IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }


     [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var  auction=await context.Auctions
        .Include(x=>x.Item)
        .OrderBy(x=>x.Item.Make)
        .ToListAsync();

        return mapper.Map<List<AuctionDto>>(auction);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x=>x.Id==id);

        if (auction == null) return NotFound();

        return mapper.Map<AuctionDto>(auction);
    }


     [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = mapper.Map<Auction>(createAuctionDto);
        auction.Seller = "test";
        context.Auctions.Add(auction);
        var result = await context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save the changes in db");
        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, mapper.Map<CreateAuctionDto>(auction));

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id,UpdateAuctionDto updateAuctionDto)
    {
        var auction = await context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x=>x.Id==id);
         if (auction == null) return NotFound();

         auction.Item.Make = updateAuctionDto.Make ??  auction.Item.Make;
         auction.Item.Model = updateAuctionDto.Model ??  auction.Item.Model;
         auction.Item.Color = updateAuctionDto.Color ??  auction.Item.Color;
         auction.Item.Year = updateAuctionDto.Year ??  auction.Item.Year;
         auction.Item.Mileage = updateAuctionDto.Mileage ??  auction.Item.Mileage;
           var result = await context.SaveChangesAsync() > 0;
           if (result) return Ok();
           return BadRequest("Problem Saving Changes");
    

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await context.Auctions.FindAsync(id);
         if (auction == null) return NotFound();

          context.Remove(auction);
           var result = await context.SaveChangesAsync() > 0;
           if (result) return Ok();
           return BadRequest("Could not update DB");    

    }

}
