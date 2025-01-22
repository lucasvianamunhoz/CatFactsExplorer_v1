import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5001/api",
});

export const getCatFacts = async () => {
    const response = await api.get("/CatFacts");
    return response.data;
};

export const addCatFact = async (catFact: { fact: string; length: number }) => {
    const response = await api.post("/CatFacts", catFact);
    return response.data;
};