namespace ShoukoV2.Models.Anilist;

public static class AnilistQueries
{
    public const string _currentUserQuery =
        @"
        query {
          Viewer {
            id
            name
            avatar {
              large
            }
            bannerImage
          }
        }";
    
    public const string _currentUserStatistics =
        @"
        query {
          Viewer {
            id
            name
            avatar {
              large
            }
            bannerImage
            statistics {
              anime {
                count
                meanScore
                volumesRead
              }
              manga {
                chaptersRead
                count
                meanScore
              }
            }
          }
        }";
}