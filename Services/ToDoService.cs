using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDo.ToDoBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if(request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must provide a valid object"));


        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(toDoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse{
            Id = toDoItem.Id
        });
    
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0) 
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is invalid"));

        var toDoItem = await _dbContext.toDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if(toDoItem != null) 
        {
            return await Task.FromResult(new ReadToDoResponse
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.Status
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id} was found"));
    }


    public override async Task<GetAllResponse> GetAllToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var toDoItem = await _dbContext.toDoItems.ToListAsync();
        foreach(var todo in toDoItem)
        {
            response.ToDo.Add(new ReadToDoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                ToDoStatus = todo.Status
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Provide complete update request"));

        var toDoItem = await _dbContext.toDoItems.FirstOrDefaultAsync(t=> t.Id == request.Id);
        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id} was found"));
        
        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.Status = request.ToDoStatus;

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = toDoItem.Id
        });
    
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));
        var toDoItem = await _dbContext.toDoItems.FirstOrDefaultAsync(t=> t.Id == request.Id);
        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id} was found"));
        
        _dbContext.Remove(toDoItem);
        await _dbContext.SaveChangesAsync();
        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = toDoItem.Id
        });
    }

}