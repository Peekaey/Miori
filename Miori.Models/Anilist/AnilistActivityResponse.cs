public class AniListActivityResponse
{
    public ActivityData Data { get; set; }
}

public class ActivityData
{
    public ActivityPage Page { get; set; }
}

public class ActivityPage
{
    public PageInfo PageInfo { get; set; }
    public List<ActivityBase> Activities { get; set; }
}

public class PageInfo
{
    public int Total { get; set; }
    public int PerPage { get; set; }
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public bool HasNextPage { get; set; }
}

public class ActivityBase
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int CreatedAt { get; set; }
    
    public string Message { get; set; }
    public User Messenger { get; set; }
    public User Recipient { get; set; }
    
    public string Status { get; set; }
    public string Progress { get; set; }
    public Media Media { get; set; }
    public User User { get; set; }
    public List<User> Likes { get; set; }
    public List<Reply> Replies { get; set; }
    
    public string Text { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Avatar Avatar { get; set; }
}

public class Avatar
{
    public string Large { get; set; }
    public string Medium { get; set; }
}

public class Media
{
    public int Id { get; set; }
    public MediaTitle Title { get; set; }
    public CoverImage CoverImage { get; set; }
    public string Type { get; set; }
}

public class MediaTitle
{
    public string Romaji { get; set; }
    public string English { get; set; }
    public string Native { get; set; }
}

public class CoverImage
{
    public string Large { get; set; }
}

public class Reply
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int CreatedAt { get; set; }
    public User User { get; set; }
}