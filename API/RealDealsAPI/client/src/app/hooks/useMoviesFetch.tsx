import { useEffect } from "react";
import { useAppSelector, useAppDispatch } from "../store/configureStore";
import { fetchMoviesAsync, movieSelectors } from "../../features/movieCatalog/movieCatalogSlice";

export default function useMoviesFetch() {
    const movies = useAppSelector(movieSelectors.selectAll);
    const { moviesLoaded } = useAppSelector(state => state.movieCatalog);
    const dispatch = useAppDispatch();

    useEffect(() => {
        if (!moviesLoaded) dispatch(fetchMoviesAsync());
    }, [moviesLoaded, dispatch])

    return {
        movies,
        moviesLoaded
    }
}