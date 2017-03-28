package com.baidu;

public class Info_list {
	private int image;
	private String name;
	private String adress;
	private String phonenum;

	public Info_list(int image, String name, String adress, String phonenum) {
		// TODO Auto-generated constructor stub
		this.image = image;
		this.name = name;
		this.phonenum = phonenum;
		this.adress = adress;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public String getAdress() {
		return adress;
	}

	public void setAdress(String adress) {
		this.adress = adress;
	}

	public String getPhonenum() {
		return phonenum;
	}

	public void setPhonenum(String phonenum) {
		this.phonenum = phonenum;
	}

	public int getImage() {
		return image;
	}

	public void setImage(int image) {
		this.image = image;
	}

}
