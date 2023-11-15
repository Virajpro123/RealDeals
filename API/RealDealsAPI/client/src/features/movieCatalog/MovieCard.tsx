import { Button, Card, CardActions, CardContent, CardHeader, CardMedia, Typography } from "@mui/material";
import { Movie } from "../../app/models/movie";
import BestDealPopup from "./BestDealPopup";
import { useState } from "react";

interface Props {
    movie: Movie;
}

export default function MovieCard({ movie }: Props) {
    const [isPopupOpen, setIsPopupOpen] = useState(false);
    const handleOpenPopup = () => {
        setIsPopupOpen(true);
      };
      const closeOpenPopup = () => {
        setIsPopupOpen(false);
      };
    return (
        <Card>
            <CardHeader
                title={movie.title}
                titleTypographyProps={{
                    sx: { textAlign:'center', fontWeight: 'bold', color: '#003049' }
                }}
            />
            <CardMedia
                sx={{ height: 500, backgroundSize: 'contain', bgcolor: 'primary.light' }}
                image={movie.poster}
                title={movie.title}
            />
            <CardContent>
                <Typography variant="h6" style={{fontWeight: 'bold'}} color="text.secondary">
                    {movie.year}
                </Typography>
                <CardActions style={{justifyContent: 'center'}}>
                <Button onClick={handleOpenPopup} color='secondary' variant="contained" data-testid="getDealButton" size="large">Check Deals</Button>
                {isPopupOpen && <BestDealPopup isOpen={isPopupOpen} popupToggle={closeOpenPopup} relatedIds={movie.relatedIDs}/>}
                </CardActions>
            </CardContent>
        </Card>
    )
}