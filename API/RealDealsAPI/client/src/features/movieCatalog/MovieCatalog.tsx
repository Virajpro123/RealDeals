import { Grid } from "@mui/material";
import MovieList from "./MovieList";
import useMoviesFetch from "../../app/hooks/useMoviesFetch";
import LoadingComponent from "../../app/layout/LoadingComponent";

export default function MovieCatalog() {
    const {movies,moviesLoaded} = useMoviesFetch();

    if (!moviesLoaded) return <LoadingComponent message={'Loading Movies...'} isBackDropInvisible= {true} />

    return (
        <Grid container >
            <Grid item>
                <MovieList movies={movies} />
            </Grid>
            <Grid item  />
        </Grid>
    )
}