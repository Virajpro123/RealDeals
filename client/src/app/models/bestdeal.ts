export interface BestDeal {
    movieDetails: MovieDetails;
    isRealTime: boolean;
    provider: string;
  }
  
  export interface MovieDetails {
    title: string;
    year: string;
    rated: string;
    released: string;
    runtime: string;
    genre: string;
    director: string;
    writer: string;
    actors: string;
    plot: string;
    language: string;
    country: string;
    awards: string;
    poster: string;
    metascore: string;
    rating: string;
    votes: string;
    id: string;
    type: string;
    price: string;
  }