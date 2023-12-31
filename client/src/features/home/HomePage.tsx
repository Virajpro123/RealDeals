import { Box, Button, Typography } from "@mui/material";
import RealDeaLsLogo from "../../assets/HomePageLogo.png"
import { router } from "../../app/router/Routes";
import LoadingComponent from "../../app/layout/LoadingComponent";
import { useState } from "react";

export default function HomePage() {

    const [loading, setLoading] = useState(true);

    const handleImageLoad = () => {
        setLoading(false);
    };

    const style = {
        display: 'flex',
        width: '100%',
        transform: 'translate(0%, -10%)',
    };


    return (
        <Box>
            {loading && <LoadingComponent message={'Loading...'} isBackDropInvisible={true} />}
            <img
                src={RealDeaLsLogo}
                alt={'Reel Deals Logo'}
                onLoad={handleImageLoad}
                style={style}
            />

            {!loading &&
                <>
                    <Typography variant='h1' justifyContent='center' style={{ display: 'flex', width: '100%', transform: 'translate(0%, -150%)', fontWeight: 'bold' }}>
                        Welcome..!
                    </Typography>
                    <Box display='flex' justifyContent='center' sx={{ p: 4 }} >
                        <Button onClick={() => { router.navigate('/Catalog') }} style={{ color: '#FFFFFF', background: '#1A2C50', transform: 'translate(0%, -380%)' }} size='large' variant="contained" data-testid="goToCatalog" >Go to Movie Catalog</Button>
                    </Box>
                </>
            }
        </Box>
    )
}