import { render, screen } from '@testing-library/react';
import MovieList from './MovieList';
import { Movie } from '../../app/models/movie';
import React from 'react';

jest.mock('../../app/store/configureStore', () => ({
  useAppSelector: jest.fn(),
}));

describe('MovieList', () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  it('renders MovieList with MovieCard when movies are loaded', () => {
    const mockMovies: Movie[] = [
      { title: 'Movie 1', year: '2022', relatedIDs: '1', type: 'Action', poster: 'poster1.jpg', providers: 'Netflix' },
      { title: 'Movie 2', year: '2021', relatedIDs: '2', type: 'Drama', poster: 'poster2.jpg', providers: 'Amazon Prime' },
    ];

    const useSelectorMock = jest.fn();
    useSelectorMock.mockReturnValue({ moviesLoaded: true });
    render(<MovieList movies={mockMovies} />);

    mockMovies.forEach((movie) => {
      expect(screen.getByText(movie.title)).toBeInTheDocument();
      expect(screen.getByText(movie.year)).toBeInTheDocument();
      expect(screen.getByText(movie.type)).toBeInTheDocument();
    });

    expect(
      screen.queryByText(/Could not Fetch Movies! please try again later../i)
    ).not.toBeInTheDocument();
  });
  
});