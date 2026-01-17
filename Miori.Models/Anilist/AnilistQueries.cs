namespace Miori.Models.Anilist;

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
            siteUrl
            avatar {
              large
            }
            bannerImage
            statistics {
              anime {
                count
                meanScore
                episodesWatched
                minutesWatched
              }
              manga {
                chaptersRead
                volumesRead
                count
                meanScore
              }
            }
          }
        }";
    
        public const string _userActivityQuery = @"
      query ($userId: Int, $page: Int, $perPage: Int) {
        Page(page: $page, perPage: $perPage) {
          pageInfo {
            total
            perPage
            currentPage
            lastPage
            hasNextPage
          }
          activities(userId: $userId, sort: ID_DESC) {
            ... on TextActivity {
              id
              type
              text
              createdAt
              user {
                id
                name
                avatar {
                  large
                }
              }
              likes {
                id
                name
              }
              replies {
                id
                text
                createdAt
                user {
                  id
                  name
                }
              }
            }
            ... on ListActivity {
              id
              type
              status
              progress
              createdAt
              media {
                id
                title {
                  romaji
                  english
                  native
                }
                coverImage {
                  large
                }
                type
              }
              user {
                id
                name
                avatar {
                  large
                }
              }
              likes {
                id
                name
              }
              replies {
                id
                text
                createdAt
                user {
                  id
                  name
                }
              }
            }
            ... on MessageActivity {
              id
              type
              message
              createdAt
              messenger {
                id
                name
                avatar {
                  large
                }
              }
              recipient {
                id
                name
              }
            }
          }
        }
      }
      ";
}