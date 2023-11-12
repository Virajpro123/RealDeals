import { Grid, Typography } from "@mui/material";
import { useAppSelector } from "../../app/store/configureStore";
import { Movie } from "../../app/models/movie";
import MovieCard from "./MovieCard";

interface Props {
    movies: Movie[];
}

export default function MovieList({ movies }: Props) {
    const { moviesLoaded } = useAppSelector(state => state.movieCatalog);
    return (
        <Grid container spacing={5}>
            {movies.map(movie => (
                <Grid key={movie.title} item xs={12}>
                    {!moviesLoaded ? (
                        <Typography variant='h2' sx={{ display: 'flex', justifyContent: "center", position: "fixed", top: "60%", color: "red" }}>Could not Fetch Movies! please try again later..</Typography>
                    ) : (
                        <MovieCard movie={movie} />
                    )}
                </Grid>
            ))}
        </Grid>
    )
}